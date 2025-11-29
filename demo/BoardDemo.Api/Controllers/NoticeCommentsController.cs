using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 공지사항 댓글 컨트롤러
/// 공지사항에 대한 댓글만 관리 (읽기 전용 - 댓글 작성 불가)
/// </summary>
[Route("api/notices")]
[Tags("공지사항 댓글")]
public class NoticeCommentsController : CommentsController
{
    public NoticeCommentsController(
        ICommentService commentService,
        ILikeService likeService,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
        : base(commentService, likeService, createValidator, updateValidator)
    {
    }

    /// <summary>
    /// 공지사항 댓글 목록 조회
    /// </summary>
    [HttpGet("{postId:long}/comments")]
    public override async Task<ActionResult<PagedResponse<CommentResponse>>> GetByPostId(
        long postId,
        [FromQuery] CommentQueryParameters parameters)
    {
        return await base.GetByPostId(postId, parameters);
    }

    /// <summary>
    /// 공지사항 댓글 작성 (비활성화)
    /// </summary>
    [HttpPost("{postId:long}/comments")]
    public override async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
        long postId,
        [FromBody] CreateCommentRequest request)
    {
        // 공지사항에는 댓글 작성 불가
        return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
            "COMMENTS_DISABLED",
            "공지사항에는 댓글을 작성할 수 없습니다."));
    }
}
