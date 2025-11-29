using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BoardDemo.Api.Data;
using BoardDemo.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BoardDemo.Api.Services;

/// <summary>
/// 인증 서비스 인터페이스
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByIdAsync(long userId);
    Task<UserDto?> ValidateTokenAsync(string token);
}

/// <summary>
/// 인증 서비스 구현
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 회원가입
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // 이메일 중복 확인
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "이미 사용 중인 이메일입니다."
                };
            }

            // 사용자명 중복 확인
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "이미 사용 중인 사용자명입니다."
                };
            }

            // 사용자 생성
            var user = new AppUser
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName ?? request.Username,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 토큰 생성
            var tokens = await GenerateTokensAsync(user);

            _logger.LogInformation("User registered: {Email}", user.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "회원가입이 완료되었습니다.",
                User = MapToUserDto(user),
                Tokens = tokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "회원가입 중 오류가 발생했습니다."
            };
        }
    }

    /// <summary>
    /// 로그인
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "이메일 또는 비밀번호가 올바르지 않습니다."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "이메일 또는 비밀번호가 올바르지 않습니다."
                };
            }

            // 마지막 로그인 시간 업데이트
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 토큰 생성
            var tokens = await GenerateTokensAsync(user);

            _logger.LogInformation("User logged in: {Email}", user.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "로그인되었습니다.",
                User = MapToUserDto(user),
                Tokens = tokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "로그인 중 오류가 발생했습니다."
            };
        }
    }

    /// <summary>
    /// 토큰 갱신
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "유효하지 않은 리프레시 토큰입니다."
                };
            }

            if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "만료되었거나 취소된 리프레시 토큰입니다."
                };
            }

            // 기존 토큰 무효화
            storedToken.IsRevoked = true;

            // 새 토큰 생성
            var tokens = await GenerateTokensAsync(storedToken.User);

            _logger.LogInformation("Token refreshed for user: {UserId}", storedToken.UserId);

            return new AuthResponse
            {
                Success = true,
                Message = "토큰이 갱신되었습니다.",
                User = MapToUserDto(storedToken.User),
                Tokens = tokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return new AuthResponse
            {
                Success = false,
                Message = "토큰 갱신 중 오류가 발생했습니다."
            };
        }
    }

    /// <summary>
    /// 토큰 취소 (로그아웃)
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
            return false;

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// ID로 사용자 조회
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null ? null : MapToUserDto(user);
    }

    /// <summary>
    /// 토큰 유효성 검증
    /// </summary>
    public async Task<UserDto?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdClaim, out var userId))
            {
                return await GetUserByIdAsync(userId);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #region Private Methods

    /// <summary>
    /// Access Token과 Refresh Token 생성
    /// </summary>
    private async Task<TokenDto> GenerateTokensAsync(AppUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            RefreshTokenExpires = refreshToken.ExpiresAt
        };
    }

    /// <summary>
    /// JWT Access Token 생성
    /// </summary>
    private string GenerateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName ?? user.Username)
        };

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Refresh Token 생성 및 저장
    /// </summary>
    private async Task<RefreshToken> GenerateRefreshTokenAsync(long userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7일 유효
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// AppUser를 UserDto로 변환
    /// </summary>
    private static UserDto MapToUserDto(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}
