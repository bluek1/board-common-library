using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 게시물 API 컨트롤러
/// 상속하여 커스터마이징 가능합니다.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    protected readonly IPostService PostService;
    protected readonly IViewCountService ViewCountService;
    protected readonly ILikeService LikeService;
    protected readonly IBookmarkService BookmarkService;
    protected readonly IValidator<CreatePostRequest> CreateValidator;
    protected readonly IValidator<UpdatePostRequest> UpdateValidator;
    protected readonly IValidator<DraftPostRequest> DraftValidator;
    
    public PostsController(
        IPostService postService,
        IViewCountService viewCountService,
        ILikeService likeService,
        IBookmarkService bookmarkService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator)
    {
        PostService = postService;
        ViewCountService = viewCountService;
        LikeService = likeService;
        BookmarkService = bookmarkService;
        CreateValidator = createValidator;
        UpdateValidator = updateValidator;
        DraftValidator = draftValidator;
    }
    
    /// <summary>
    /// 게시물 목록 조회
    /// </summary>
    /// <remarks>
    /// 페이징, 정렬, 필터링이 적용된 게시물 목록을 조회합니다.
    /// 상단고정 게시물이 먼저 표시됩니다.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PostSummaryResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<PagedResponse<PostSummaryResponse>>> GetAll(
        [FromQuery] PostQueryParameters parameters)
    {
        var result = await PostService.GetAllAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 게시물 상세 조회
    /// </summary>
    /// <remarks>
    /// 게시물 ID로 상세 정보를 조회합니다.
    /// 조회 시 조회수가 자동으로 증가합니다 (24시간 내 중복 조회 방지).
    /// </remarks>
    /// <param name="id">게시물 ID</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> GetById(long id)
    {
        var post = await PostService.GetByIdAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 조회수 증가
        var userId = GetCurrentUserId();
        var ipAddress = GetClientIpAddress();
        await ViewCountService.IncrementViewCountAsync(id, userId, ipAddress);
        
        // 조회수 갱신
        post.ViewCount = await ViewCountService.GetViewCountAsync(id);
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }
    
    /// <summary>
    /// 게시물 작성
    /// </summary>
    /// <remarks>
    /// 새 게시물을 작성합니다.
    /// </remarks>
    /// <param name="request">게시물 생성 요청</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> Create([FromBody] CreatePostRequest request)
    {
        // 유효성 검증
        var validationResult = await CreateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationError { Field = e.PropertyName, Message = e.ErrorMessage })
                .ToList();
            
            return BadRequest(ApiErrorResponse.Create(
                "VALIDATION_ERROR",
                "입력값이 유효하지 않습니다.",
                errors));
        }
        
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var authorName = GetCurrentUserName();
        var post = await PostService.CreateAsync(request, userId.Value, authorName);
        
        return CreatedAtAction(
            nameof(GetById), 
            new { id = post.Id }, 
            ApiResponse<PostResponse>.Ok(post));
    }
    
    /// <summary>
    /// 게시물 수정
    /// </summary>
    /// <remarks>
    /// 기존 게시물을 수정합니다. 작성자 또는 관리자만 수정 가능합니다.
    /// </remarks>
    /// <param name="id">게시물 ID</param>
    /// <param name="request">게시물 수정 요청</param>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        // 유효성 검증
        var validationResult = await UpdateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationError { Field = e.PropertyName, Message = e.ErrorMessage })
                .ToList();
            
            return BadRequest(ApiErrorResponse.Create(
                "VALIDATION_ERROR",
                "입력값이 유효하지 않습니다.",
                errors));
        }
        
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        // 게시물 존재 여부 확인
        if (!await PostService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        var isAdmin = IsCurrentUserAdmin();
        var isAuthor = await PostService.IsAuthorAsync(id, userId.Value);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "게시물을 수정할 권한이 없습니다."));
        }
        
        var post = await PostService.UpdateAsync(id, request, userId.Value, isAdmin);
        
        return Ok(ApiResponse<PostResponse>.Ok(post!));
    }
    
    /// <summary>
    /// 게시물 삭제
    /// </summary>
    /// <remarks>
    /// 게시물을 삭제합니다 (소프트 삭제). 작성자 또는 관리자만 삭제 가능합니다.
    /// </remarks>
    /// <param name="id">게시물 ID</param>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult> Delete(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        // 게시물 존재 여부 확인
        if (!await PostService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        var isAdmin = IsCurrentUserAdmin();
        var isAuthor = await PostService.IsAuthorAsync(id, userId.Value);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "게시물을 삭제할 권한이 없습니다."));
        }
        
        await PostService.DeleteAsync(id, userId.Value, isAdmin);
        
        return NoContent();
    }
    
    /// <summary>
    /// 게시물 상단고정 설정
    /// </summary>
    /// <remarks>
    /// 게시물을 목록 상단에 고정합니다. 관리자만 가능합니다.
    /// </remarks>
    /// <param name="id">게시물 ID</param>
    [HttpPost("{id:long}/pin")]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> Pin(long id)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "관리자만 상단고정할 수 있습니다."));
        }
        
        var post = await PostService.PinAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }
    
    /// <summary>
    /// 게시물 상단고정 해제
    /// </summary>
    /// <remarks>
    /// 게시물의 상단고정을 해제합니다. 관리자만 가능합니다.
    /// </remarks>
    /// <param name="id">게시물 ID</param>
    [HttpDelete("{id:long}/pin")]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> Unpin(long id)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "관리자만 상단고정을 해제할 수 있습니다."));
        }
        
        var post = await PostService.UnpinAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }
    
    /// <summary>
    /// 임시저장
    /// </summary>
    /// <remarks>
    /// 작성 중인 게시물을 임시 저장합니다.
    /// ExistingDraftId를 제공하면 기존 임시저장을 덮어씁니다.
    /// </remarks>
    /// <param name="request">임시저장 요청</param>
    [HttpPost("draft")]
    [ProducesResponseType(typeof(ApiResponse<DraftPostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public virtual async Task<ActionResult<ApiResponse<DraftPostResponse>>> SaveDraft([FromBody] DraftPostRequest request)
    {
        // 유효성 검증
        var validationResult = await DraftValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationError { Field = e.PropertyName, Message = e.ErrorMessage })
                .ToList();
            
            return BadRequest(ApiErrorResponse.Create(
                "VALIDATION_ERROR",
                "입력값이 유효하지 않습니다.",
                errors));
        }
        
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var authorName = GetCurrentUserName();
        var draft = await PostService.SaveDraftAsync(request, userId.Value, authorName);
        
        return Ok(ApiResponse<DraftPostResponse>.Ok(draft));
    }
    
    /// <summary>
    /// 임시저장 목록 조회
    /// </summary>
    /// <remarks>
    /// 현재 로그인한 사용자의 임시저장 목록을 조회합니다.
    /// </remarks>
    [HttpGet("draft")]
    [ProducesResponseType(typeof(PagedResponse<DraftPostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public virtual async Task<ActionResult<PagedResponse<DraftPostResponse>>> GetDrafts([FromQuery] PagedRequest parameters)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var drafts = await PostService.GetDraftsAsync(userId.Value, parameters);
        
        return Ok(drafts);
    }
    
    /// <summary>
    /// 임시저장 발행
    /// </summary>
    /// <remarks>
    /// 임시저장된 게시물을 정식으로 발행합니다.
    /// </remarks>
    /// <param name="id">임시저장 ID</param>
    [HttpPost("draft/{id:long}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<PostResponse>>> PublishDraft(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var post = await PostService.PublishAsync(id, userId.Value);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "DRAFT_NOT_FOUND",
                "임시저장을 찾을 수 없거나 권한이 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }
    
    #region 좋아요
    
    /// <summary>
    /// 게시물 좋아요
    /// </summary>
    /// <param name="id">게시물 ID</param>
    [HttpPost("{id:long}/like")]
    [ProducesResponseType(typeof(ApiResponse<LikeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public virtual async Task<ActionResult<ApiResponse<LikeResponse>>> LikePost(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        try
        {
            var result = await LikeService.LikePostAsync(id, userId.Value);
            return Ok(ApiResponse<LikeResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("찾을 수 없습니다"))
            {
                return NotFound(ApiErrorResponse.Create(
                    "POST_NOT_FOUND",
                    ex.Message));
            }
            
            return Conflict(ApiErrorResponse.Create(
                "ALREADY_LIKED",
                ex.Message));
        }
    }
    
    /// <summary>
    /// 게시물 좋아요 취소
    /// </summary>
    /// <param name="id">게시물 ID</param>
    [HttpDelete("{id:long}/like")]
    [ProducesResponseType(typeof(ApiResponse<LikeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<LikeResponse>>> UnlikePost(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var result = await LikeService.UnlikePostAsync(id, userId.Value);
        
        if (result == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "NOT_FOUND",
                "좋아요하지 않은 게시물이거나 존재하지 않는 게시물입니다."));
        }
        
        return Ok(ApiResponse<LikeResponse>.Ok(result));
    }
    
    #endregion
    
    #region 북마크
    
    /// <summary>
    /// 게시물 북마크
    /// </summary>
    /// <param name="id">게시물 ID</param>
    [HttpPost("{id:long}/bookmark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public virtual async Task<ActionResult> AddBookmark(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        try
        {
            await BookmarkService.AddBookmarkAsync(id, userId.Value);
            return Ok(ApiResponse<object>.Ok(new { message = "북마크가 추가되었습니다." }));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("찾을 수 없습니다"))
            {
                return NotFound(ApiErrorResponse.Create(
                    "POST_NOT_FOUND",
                    ex.Message));
            }
            
            return Conflict(ApiErrorResponse.Create(
                "ALREADY_BOOKMARKED",
                ex.Message));
        }
    }
    
    /// <summary>
    /// 게시물 북마크 해제
    /// </summary>
    /// <param name="id">게시물 ID</param>
    [HttpDelete("{id:long}/bookmark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult> RemoveBookmark(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var result = await BookmarkService.RemoveBookmarkAsync(id, userId.Value);
        
        if (!result)
        {
            return NotFound(ApiErrorResponse.Create(
                "NOT_FOUND",
                "북마크하지 않은 게시물입니다."));
        }
        
        return Ok(ApiResponse<object>.Ok(new { message = "북마크가 해제되었습니다." }));
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// 현재 로그인한 사용자 ID 조회 (테스트용 헤더 또는 Claims에서)
    /// 오버라이드하여 커스텀 인증 로직 구현 가능
    /// </summary>
    protected virtual long? GetCurrentUserId()
    {
        // 테스트용: X-User-Id 헤더에서 조회
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        
        // TODO: JWT Claims에서 조회
        // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        // if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        // {
        //     return userId;
        // }
        
        return null;
    }
    
    /// <summary>
    /// 현재 로그인한 사용자명 조회
    /// 오버라이드하여 커스텀 인증 로직 구현 가능
    /// </summary>
    protected virtual string? GetCurrentUserName()
    {
        // 테스트용: X-User-Name 헤더에서 조회
        if (Request.Headers.TryGetValue("X-User-Name", out var userNameHeader))
        {
            return userNameHeader;
        }
        
        return null;
    }
    
    /// <summary>
    /// 현재 사용자가 관리자인지 확인
    /// 오버라이드하여 커스텀 권한 로직 구현 가능
    /// </summary>
    protected virtual bool IsCurrentUserAdmin()
    {
        // 테스트용: X-User-Role 헤더에서 조회
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().ToLower() == "admin";
        }
        
        // TODO: JWT Claims에서 조회
        // return User.IsInRole("Admin");
        
        return false;
    }
    
    /// <summary>
    /// 클라이언트 IP 주소 조회
    /// </summary>
    protected virtual string? GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // X-Forwarded-For 헤더 확인 (프록시/로드밸런서 뒤에 있을 경우)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ipAddress = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
        }
        
        return ipAddress;
    }
    
    #endregion
}
