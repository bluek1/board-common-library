using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 자유게시판 댓글 컨트롤러
/// 자유게시판에 대한 댓글 관리 (대댓글 깊이 제한, 욕설 필터링)
/// </summary>
[Route("api/free-board")]
[Tags("자유게시판 댓글")]
public class FreeBoardCommentsController : CommentsController
{
    // 금지어 목록
    private static readonly HashSet<string> ForbiddenWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "욕설1", "욕설2", "비속어", "스팸", "광고"
    };

    public FreeBoardCommentsController(
        ICommentService commentService,
        ILikeService likeService,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
        : base(commentService, likeService, createValidator, updateValidator)
    {
    }

    /// <summary>
    /// 자유게시판 댓글 목록 조회
    /// </summary>
    [HttpGet("{postId:long}/comments")]
    public override async Task<ActionResult<PagedResponse<CommentResponse>>> GetByPostId(
        long postId,
        [FromQuery] CommentQueryParameters parameters)
    {
        return await base.GetByPostId(postId, parameters);
    }

    /// <summary>
    /// 자유게시판 댓글 작성 (금지어 필터링 적용)
    /// </summary>
    [HttpPost("{postId:long}/comments")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
        long postId,
        [FromBody] CreateCommentRequest request)
    {
        // 금지어 검증
        if (ContainsForbiddenWords(request.Content))
        {
            return BadRequest(ApiErrorResponse.Create(
                "FORBIDDEN_WORDS",
                "댓글에 금지어가 포함되어 있습니다."));
        }

        return await base.Create(postId, request);
    }

    /// <summary>
    /// 자유게시판 대댓글 작성 (대댓글 깊이 제한 + 금지어 필터링)
    /// </summary>
    [HttpPost("comments/{parentId:long}/replies")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> CreateReply(
        long parentId,
        [FromBody] CreateCommentRequest request)
    {
        // 금지어 검증
        if (ContainsForbiddenWords(request.Content))
        {
            return BadRequest(ApiErrorResponse.Create(
                "FORBIDDEN_WORDS",
                "댓글에 금지어가 포함되어 있습니다."));
        }

        // 대댓글 깊이 제한 (1단계만 허용)
        var parentComment = await CommentService.GetByIdAsync(parentId);
        if (parentComment == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "COMMENT_NOT_FOUND",
                "부모 댓글을 찾을 수 없습니다."));
        }

        if (parentComment.ParentId.HasValue)
        {
            return BadRequest(ApiErrorResponse.Create(
                "MAX_DEPTH_EXCEEDED",
                "자유게시판은 1단계 대댓글까지만 허용됩니다."));
        }

        return await base.CreateReply(parentId, request);
    }

    /// <summary>
    /// 자유게시판 댓글 수정 (금지어 필터링 적용)
    /// </summary>
    [HttpPut("comments/{id:long}")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> Update(
        long id,
        [FromBody] UpdateCommentRequest request)
    {
        // 금지어 검증
        if (ContainsForbiddenWords(request.Content))
        {
            return BadRequest(ApiErrorResponse.Create(
                "FORBIDDEN_WORDS",
                "댓글에 금지어가 포함되어 있습니다."));
        }

        return await base.Update(id, request);
    }

    /// <summary>
    /// 금지어 포함 여부 확인
    /// </summary>
    private static bool ContainsForbiddenWords(string content)
    {
        if (string.IsNullOrEmpty(content)) return false;
        
        return ForbiddenWords.Any(word => content.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}
