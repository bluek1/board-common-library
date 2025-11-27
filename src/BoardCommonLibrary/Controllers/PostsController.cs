using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 게시물 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IViewCountService _viewCountService;
    private readonly IValidator<CreatePostRequest> _createValidator;
    private readonly IValidator<UpdatePostRequest> _updateValidator;
    private readonly IValidator<DraftPostRequest> _draftValidator;
    
    public PostsController(
        IPostService postService,
        IViewCountService viewCountService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator)
    {
        _postService = postService;
        _viewCountService = viewCountService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _draftValidator = draftValidator;
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
    public async Task<ActionResult<PagedResponse<PostSummaryResponse>>> GetAll(
        [FromQuery] PostQueryParameters parameters)
    {
        var result = await _postService.GetAllAsync(parameters);
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> GetById(long id)
    {
        var post = await _postService.GetByIdAsync(id);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 조회수 증가
        var userId = GetCurrentUserId();
        var ipAddress = GetClientIpAddress();
        await _viewCountService.IncrementViewCountAsync(id, userId, ipAddress);
        
        // 조회수 갱신
        post.ViewCount = await _viewCountService.GetViewCountAsync(id);
        
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> Create([FromBody] CreatePostRequest request)
    {
        // 유효성 검증
        var validationResult = await _createValidator.ValidateAsync(request);
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
        var post = await _postService.CreateAsync(request, userId.Value, authorName);
        
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        // 유효성 검증
        var validationResult = await _updateValidator.ValidateAsync(request);
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
        if (!await _postService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        var isAdmin = IsCurrentUserAdmin();
        var isAuthor = await _postService.IsAuthorAsync(id, userId.Value);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "게시물을 수정할 권한이 없습니다."));
        }
        
        var post = await _postService.UpdateAsync(id, request, userId.Value, isAdmin);
        
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
    public async Task<ActionResult> Delete(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        // 게시물 존재 여부 확인
        if (!await _postService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                "게시물을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        var isAdmin = IsCurrentUserAdmin();
        var isAuthor = await _postService.IsAuthorAsync(id, userId.Value);
        
        if (!isAuthor && !isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "게시물을 삭제할 권한이 없습니다."));
        }
        
        await _postService.DeleteAsync(id, userId.Value, isAdmin);
        
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> Pin(long id)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "관리자만 상단고정할 수 있습니다."));
        }
        
        var post = await _postService.PinAsync(id);
        
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> Unpin(long id)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "관리자만 상단고정을 해제할 수 있습니다."));
        }
        
        var post = await _postService.UnpinAsync(id);
        
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
    public async Task<ActionResult<ApiResponse<DraftPostResponse>>> SaveDraft([FromBody] DraftPostRequest request)
    {
        // 유효성 검증
        var validationResult = await _draftValidator.ValidateAsync(request);
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
        var draft = await _postService.SaveDraftAsync(request, userId.Value, authorName);
        
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
    public async Task<ActionResult<PagedResponse<DraftPostResponse>>> GetDrafts([FromQuery] PagedRequest parameters)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var drafts = await _postService.GetDraftsAsync(userId.Value, parameters);
        
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
    public async Task<ActionResult<ApiResponse<PostResponse>>> PublishDraft(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var post = await _postService.PublishAsync(id, userId.Value);
        
        if (post == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "DRAFT_NOT_FOUND",
                "임시저장을 찾을 수 없거나 권한이 없습니다."));
        }
        
        return Ok(ApiResponse<PostResponse>.Ok(post));
    }
    
    #region Helper Methods
    
    /// <summary>
    /// 현재 로그인한 사용자 ID 조회 (테스트용 헤더 또는 Claims에서)
    /// </summary>
    private long? GetCurrentUserId()
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
    /// </summary>
    private string? GetCurrentUserName()
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
    /// </summary>
    private bool IsCurrentUserAdmin()
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
    private string? GetClientIpAddress()
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
