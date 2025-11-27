using BoardCommonLibrary.Data;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 조회수 서비스 구현
/// </summary>
public class ViewCountService : IViewCountService
{
    private readonly BoardDbContext _context;
    
    /// <summary>
    /// 중복 체크 기간 (24시간)
    /// </summary>
    private static readonly TimeSpan DuplicateCheckPeriod = TimeSpan.FromHours(24);
    
    public ViewCountService(BoardDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<bool> IncrementViewCountAsync(long postId, long? userId, string? ipAddress)
    {
        // 중복 체크
        if (await HasViewedAsync(postId, userId, ipAddress))
        {
            return false;
        }
        
        // 조회 기록 저장
        var viewRecord = new ViewRecord
        {
            PostId = postId,
            UserId = userId,
            IpAddress = ipAddress,
            ViewedAt = DateTime.UtcNow
        };
        
        _context.ViewRecords.Add(viewRecord);
        
        // 게시물 조회수 증가
        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
        {
            post.ViewCount++;
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<int> GetViewCountAsync(long postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        return post?.ViewCount ?? 0;
    }
    
    /// <inheritdoc />
    public async Task<bool> HasViewedAsync(long postId, long? userId, string? ipAddress)
    {
        var cutoffTime = DateTime.UtcNow - DuplicateCheckPeriod;
        
        // 로그인 사용자: UserId 기준
        if (userId.HasValue)
        {
            return await _context.ViewRecords
                .AnyAsync(v => v.PostId == postId && 
                              v.UserId == userId.Value && 
                              v.ViewedAt > cutoffTime);
        }
        
        // 비로그인 사용자: IP 주소 기준
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            return await _context.ViewRecords
                .AnyAsync(v => v.PostId == postId && 
                              v.IpAddress == ipAddress && 
                              v.ViewedAt > cutoffTime);
        }
        
        return false;
    }
}
