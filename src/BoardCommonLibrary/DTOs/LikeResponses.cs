namespace BoardCommonLibrary.DTOs;

/// <summary>
/// 좋아요 응답 DTO
/// </summary>
public class LikeResponse
{
    /// <summary>
    /// 좋아요 여부
    /// </summary>
    public bool IsLiked { get; set; }
    
    /// <summary>
    /// 총 좋아요 수
    /// </summary>
    public int TotalLikeCount { get; set; }
}
