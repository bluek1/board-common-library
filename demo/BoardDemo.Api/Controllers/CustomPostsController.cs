using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using BoardCommonLibrary.Services.Interfaces;
using BoardDemo.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// PostsController를 상속받아 커스터마이징한 컨트롤러
/// 기본 기능 외에 인기 게시물, 통계 등 추가 엔드포인트를 제공합니다.
/// </summary>
[Route("api/custom-posts")]
[ApiController]
public class CustomPostsController : PostsController
{
    private readonly CustomPostService _customPostService;
    private readonly ILogger<CustomPostsController> _logger;
    
    public CustomPostsController(
        IPostService postService,
        IViewCountService viewCountService,
        ILikeService likeService,
        IBookmarkService bookmarkService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator,
        ILogger<CustomPostsController> logger)
        : base(postService, viewCountService, likeService, bookmarkService, 
               createValidator, updateValidator, draftValidator)
    {
        // IPostService가 CustomPostService로 등록되어 있으므로 캐스팅
        _customPostService = (CustomPostService)postService;
        _logger = logger;
    }
    
    #region 추가 엔드포인트
    
    /// <summary>
    /// 인기 게시물 조회 (좋아요 + 조회수 기준)
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<List<PostSummaryResponse>>> GetPopularPosts([FromQuery] int count = 10)
    {
        _logger.LogInformation("🔥 [CustomPostsController] 인기 게시물 요청: count={Count}", count);
        
        if (count < 1 || count > 50)
        {
            return BadRequest(new { message = "count는 1~50 사이여야 합니다." });
        }
        
        var posts = await _customPostService.GetPopularPostsAsync(count);
        return Ok(posts);
    }
    
    /// <summary>
    /// 최근 활동 게시물 조회 (최근 댓글이 달린 게시물)
    /// </summary>
    [HttpGet("recent-active")]
    public async Task<ActionResult<List<PostSummaryResponse>>> GetRecentlyActivePosts([FromQuery] int count = 10)
    {
        _logger.LogInformation("⏰ [CustomPostsController] 최근 활동 게시물 요청: count={Count}", count);
        
        if (count < 1 || count > 50)
        {
            return BadRequest(new { message = "count는 1~50 사이여야 합니다." });
        }
        
        var posts = await _customPostService.GetRecentlyActivePostsAsync(count);
        return Ok(posts);
    }
    
    /// <summary>
    /// 게시물 통계 조회
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<PostStatistics>> GetStatistics()
    {
        _logger.LogInformation("📊 [CustomPostsController] 통계 요청");
        
        var statistics = await _customPostService.GetStatisticsAsync();
        return Ok(statistics);
    }
    
    #endregion
    
    #region 기존 메서드 오버라이드 (로깅 추가)
    
    /// <summary>
    /// 게시물 생성 - 로깅 추가
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Create([FromBody] CreatePostRequest request)
    {
        _logger.LogInformation(
            "📝 [CustomPostsController] 게시물 생성 요청: 제목='{Title}', 카테고리='{Category}'", 
            request.Title, request.Category);
        
        var result = await base.Create(request);
        
        if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is ApiResponse<PostResponse> response && response.Success)
        {
            _logger.LogInformation(
                "✅ [CustomPostsController] 게시물 생성 완료: ID={PostId}", response.Data?.Id);
        }
        
        return result;
    }
    
    /// <summary>
    /// 게시물 수정 - 로깅 추가
    /// </summary>
    [HttpPut("{id}")]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        _logger.LogInformation(
            "✏️ [CustomPostsController] 게시물 수정 요청: ID={PostId}", id);
        
        var result = await base.Update(id, request);
        
        if (result.Result is OkObjectResult)
        {
            _logger.LogInformation(
                "✅ [CustomPostsController] 게시물 수정 완료: ID={PostId}", id);
        }
        
        return result;
    }
    
    /// <summary>
    /// 게시물 삭제 - 로깅 추가
    /// </summary>
    [HttpDelete("{id}")]
    public override async Task<ActionResult> Delete(long id)
    {
        _logger.LogWarning(
            "🗑️ [CustomPostsController] 게시물 삭제 요청: ID={PostId}", id);
        
        var result = await base.Delete(id);
        
        if (result is NoContentResult)
        {
            _logger.LogWarning(
                "✅ [CustomPostsController] 게시물 삭제 완료: ID={PostId}", id);
        }
        
        return result;
    }
    
    #endregion
    
    #region 헬퍼 메서드 오버라이드
    
    /// <summary>
    /// 현재 사용자 ID 가져오기 - 커스텀 로직 예시
    /// </summary>
    protected override long? GetCurrentUserId()
    {
        // 기본 로직 사용
        var userId = base.GetCurrentUserId();
        
        // 로깅 추가 (예시)
        _logger.LogDebug("👤 [CustomPostsController] 현재 사용자 ID: {UserId}", userId);
        
        return userId;
    }
    
    #endregion
}
