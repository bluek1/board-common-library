namespace BoardDemo.Api.Models;

/// <summary>
/// 애플리케이션 사용자
/// </summary>
public class AppUser
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = "User"; // Admin, Moderator, User
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

/// <summary>
/// 리프레시 토큰
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public long UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    
    // Navigation
    public AppUser User { get; set; } = null!;
}
