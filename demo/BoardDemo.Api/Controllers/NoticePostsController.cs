using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 공지사항 게시판 컨트롤러
/// 관리자만 작성 가능, 모든 사용자 조회 가능
/// </summary>
[Route("api/notices")]
[Tags("공지사항 게시판")]
public class NoticePostsController : PostsController
{
    public NoticePostsController(
        IPostService postService,
        IViewCountService viewCountService,
        ILikeService likeService,
        IBookmarkService bookmarkService,
        IValidator<CreatePostRequest> createValidator,
        IValidator<UpdatePostRequest> updateValidator,
        IValidator<DraftPostRequest> draftValidator)
        : base(postService, viewCountService, likeService, bookmarkService,
               createValidator, updateValidator, draftValidator)
    {
    }

    /// <summary>
    /// 공지사항 작성 (관리자 전용)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Create([FromBody] CreatePostRequest request)
    {
        // 관리자 권한 확인
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                error = new
                {
                    code = "ADMIN_ONLY",
                    message = "공지사항은 관리자만 작성할 수 있습니다."
                }
            });
        }

        // 공지사항 카테고리 설정
        request.Category = "notice";
        
        // 게시물 생성 후 상단고정 처리
        var result = await base.Create(request);
        
        // 상단고정은 별도 API로 처리하거나, 생성 후 Pin API 호출 필요
        // 실제 구현에서는 PostService에서 IsPinned 처리
        
        return result;
    }

    /// <summary>
    /// 공지사항 수정 (관리자 전용)
    /// </summary>
    [HttpPut("{id:long}")]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                error = new
                {
                    code = "ADMIN_ONLY",
                    message = "공지사항은 관리자만 수정할 수 있습니다."
                }
            });
        }

        return await base.Update(id, request);
    }

    /// <summary>
    /// 공지사항 삭제 (관리자 전용)
    /// </summary>
    [HttpDelete("{id:long}")]
    public override async Task<ActionResult> Delete(long id)
    {
        if (!IsCurrentUserAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                error = new
                {
                    code = "ADMIN_ONLY",
                    message = "공지사항은 관리자만 삭제할 수 있습니다."
                }
            });
        }

        return await base.Delete(id);
    }

    /// <summary>
    /// 게시판 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetBoardInfo()
    {
        return Ok(new
        {
            name = "공지사항",
            description = "관리자 공지사항 게시판입니다.",
            allowedRoles = new[] { "Admin" },
            features = new[] { "조회", "관리자 작성/수정/삭제" }
        });
    }

    protected override bool IsCurrentUserAdmin()
    {
        if (Request.Headers.TryGetValue("X-User-Role", out var roleHeader))
        {
            return roleHeader.ToString().Equals("admin", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
