using BoardCommonLibrary.Entities.Base;

namespace BoardCommonLibrary.Entities;

/// <summary>
/// 조회수 기록 엔티티 (중복 조회 방지용)
/// </summary>
public class ViewRecord : EntityBase
{
    /// <summary>
    /// 게시물 ID
    /// </summary>
    public long PostId { get; set; }
    
    /// <summary>
    /// 사용자 ID (비회원은 null)
    /// </summary>
    public long? UserId { get; set; }
    
    /// <summary>
    /// IP 주소 (비회원용)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// 조회 일시
    /// </summary>
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 관련 게시물 (네비게이션 프로퍼티)
    /// </summary>
    public virtual Post? Post { get; set; }
}
