using System.ComponentModel.DataAnnotations;

namespace BoardDemo.Api.Models;

#region 요청 DTOs

/// <summary>
/// 회원가입 요청
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "사용자명은 필수입니다.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "사용자명은 3~50자여야 합니다.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "이메일은 필수입니다.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호는 필수입니다.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 최소 6자 이상이어야 합니다.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호 확인은 필수입니다.")]
    [Compare(nameof(Password), ErrorMessage = "비밀번호가 일치하지 않습니다.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; set; }
}

/// <summary>
/// 로그인 요청
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "이메일은 필수입니다.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호는 필수입니다.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 토큰 갱신 요청
/// </summary>
public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

#endregion

#region 응답 DTOs

/// <summary>
/// 인증 응답 (토큰 포함)
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }
    public TokenDto? Tokens { get; set; }
}

/// <summary>
/// 토큰 정보
/// </summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}

/// <summary>
/// 사용자 정보
/// </summary>
public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

#endregion
