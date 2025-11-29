using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 댓글 API 컨트롤러
/// 상속하여 커스터마이징 가능합니다.
/// </summary>
[ApiController]
[Route("api")]
public class CommentsController : ControllerBase
{
    protected readonly ICommentService CommentService;
    protected readonly ILikeService LikeService;
    protected readonly IValidator<CreateCommentRequest> CreateValidator;
    protected readonly IValidator<UpdateCommentRequest> UpdateValidator;
    
    public CommentsController(
        ICommentService commentService,
        ILikeService likeService,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
    {
        CommentService = commentService;
        LikeService = likeService;
        CreateValidator = createValidator;
        UpdateValidator = updateValidator;
    }
    
    #region 댓글 CRUD
    
    /// <summary>
    /// 게시물의 댓글 목록 조회
    /// </summary>
    /// <remarks>
    /// 게시물에 달린 댓글 목록을 조회합니다. 대댓글도 함께 조회됩니다.
    /// </remarks>
    /// <param name="postId">게시물 ID</param>
    /// <param name="parameters">쿼리 파라미터</param>
    [HttpGet("posts/{postId:long}/comments")]
    [ProducesResponseType(typeof(PagedResponse<CommentResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<PagedResponse<CommentResponse>>> GetByPostId(
        long postId, 
        [FromQuery] CommentQueryParameters parameters)
    {
        var result = await CommentService.GetByPostIdAsync(postId, parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 댓글 작성
    /// </summary>
    /// <remarks>
    /// 게시물에 새 댓글을 작성합니다.
    /// </remarks>
    /// <param name="postId">게시물 ID</param>
    /// <param name="request">댓글 생성 요청</param>
    [HttpPost("posts/{postId:long}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
        long postId, 
        [FromBody] CreateCommentRequest request)
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
        
        try
        {
            var authorName = GetCurrentUserName();
            var comment = await CommentService.CreateAsync(postId, request, userId.Value, authorName);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = comment.Id },
                ApiResponse<CommentResponse>.Ok(comment));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiErrorResponse.Create(
                "POST_NOT_FOUND",
                ex.Message));
        }
    }
    
    /// <summary>
    /// 댓글 상세 조회
    /// </summary>
    /// <param name="id">댓글 ID</param>
    [HttpGet("comments/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<CommentResponse>>> GetById(long id)
    {
        var comment = await CommentService.GetByIdAsync(id);
        
        if (comment == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "COMMENT_NOT_FOUND",
                "댓글을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<CommentResponse>.Ok(comment));
    }
    
    /// <summary>
    /// 댓글 수정
    /// </summary>
    /// <remarks>
    /// 댓글을 수정합니다. 작성자만 수정 가능합니다.
    /// </remarks>
    /// <param name="id">댓글 ID</param>
    /// <param name="request">댓글 수정 요청</param>
    [HttpPut("comments/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<CommentResponse>>> Update(
        long id, 
        [FromBody] UpdateCommentRequest request)
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
        
        try
        {
            var comment = await CommentService.UpdateAsync(id, request, userId.Value);
            
            if (comment == null)
            {
                return NotFound(ApiErrorResponse.Create(
                    "COMMENT_NOT_FOUND",
                    "댓글을 찾을 수 없습니다."));
            }
            
            return Ok(ApiResponse<CommentResponse>.Ok(comment));
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "댓글을 수정할 권한이 없습니다."));
        }
    }
    
    /// <summary>
    /// 댓글 삭제
    /// </summary>
    /// <remarks>
    /// 댓글을 삭제합니다 (소프트 삭제). 작성자 또는 관리자만 삭제 가능합니다.
    /// 대댓글이 있는 경우 내용만 "삭제된 댓글입니다"로 변경됩니다.
    /// </remarks>
    /// <param name="id">댓글 ID</param>
    [HttpDelete("comments/{id:long}")]
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
        
        try
        {
            var isAdmin = IsCurrentUserAdmin();
            var result = await CommentService.DeleteAsync(id, userId.Value, isAdmin);
            
            if (!result)
            {
                return NotFound(ApiErrorResponse.Create(
                    "COMMENT_NOT_FOUND",
                    "댓글을 찾을 수 없습니다."));
            }
            
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "댓글을 삭제할 권한이 없습니다."));
        }
    }
    
    #endregion
    
    #region 대댓글
    
    /// <summary>
    /// 대댓글 작성
    /// </summary>
    /// <remarks>
    /// 댓글에 대댓글을 작성합니다. 최대 2단계까지 지원됩니다.
    /// </remarks>
    /// <param name="id">부모 댓글 ID</param>
    /// <param name="request">댓글 생성 요청</param>
    [HttpPost("comments/{id:long}/replies")]
    [ProducesResponseType(typeof(ApiResponse<CommentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<CommentResponse>>> CreateReply(
        long id, 
        [FromBody] CreateCommentRequest request)
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
        
        try
        {
            var authorName = GetCurrentUserName();
            var reply = await CommentService.CreateReplyAsync(id, request, userId.Value, authorName);
            
            if (reply == null)
            {
                return NotFound(ApiErrorResponse.Create(
                    "COMMENT_NOT_FOUND",
                    "부모 댓글을 찾을 수 없습니다."));
            }
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = reply.Id },
                ApiResponse<CommentResponse>.Ok(reply));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiErrorResponse.Create(
                "INVALID_OPERATION",
                ex.Message));
        }
    }
    
    #endregion
    
    #region 댓글 좋아요
    
    /// <summary>
    /// 댓글 좋아요
    /// </summary>
    /// <param name="id">댓글 ID</param>
    [HttpPost("comments/{id:long}/like")]
    [ProducesResponseType(typeof(ApiResponse<LikeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public virtual async Task<ActionResult<ApiResponse<LikeResponse>>> LikeComment(long id)
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
            var result = await LikeService.LikeCommentAsync(id, userId.Value);
            return Ok(ApiResponse<LikeResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("찾을 수 없습니다"))
            {
                return NotFound(ApiErrorResponse.Create(
                    "COMMENT_NOT_FOUND",
                    ex.Message));
            }
            
            return Conflict(ApiErrorResponse.Create(
                "ALREADY_LIKED",
                ex.Message));
        }
    }
    
    /// <summary>
    /// 댓글 좋아요 취소
    /// </summary>
    /// <param name="id">댓글 ID</param>
    [HttpDelete("comments/{id:long}/like")]
    [ProducesResponseType(typeof(ApiResponse<LikeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<LikeResponse>>> UnlikeComment(long id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create(
                "UNAUTHORIZED",
                "로그인이 필요합니다."));
        }
        
        var result = await LikeService.UnlikeCommentAsync(id, userId.Value);
        
        if (result == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "NOT_FOUND",
                "좋아요하지 않은 댓글이거나 존재하지 않는 댓글입니다."));
        }
        
        return Ok(ApiResponse<LikeResponse>.Ok(result));
    }
    
    #endregion
    
    #region Helper Methods
    
    protected virtual long? GetCurrentUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) && 
            long.TryParse(userIdHeader, out var userId))
        {
            return userId;
        }
        return null;
    }
    
    protected virtual string? GetCurrentUserName()
    {
        if (Request.Headers.TryGetValue("X-User-Name", out var userNameHeader))
        {
            return userNameHeader;
        }
        return null;
    }
    
    protected virtual bool IsCurrentUserAdmin()
    {
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().ToLower() == "admin";
        }
        return false;
    }
    
    #endregion
}
