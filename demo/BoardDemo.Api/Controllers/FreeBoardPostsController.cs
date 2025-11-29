using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 자유 게시판 컨트롤러
/// 모든 회원이 자유롭게 글을 작성할 수 있는 게시판
/// </summary>
[Route("api/free-board")]
[Tags("자유 게시판")]
public class FreeBoardPostsController : PostsController
{
    // 금지 단어 목록 (실제 환경에서는 DB나 설정에서 관리)
    private static readonly HashSet<string> _forbiddenWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "광고", "홍보", "spam", "advertisement"
    };

    public FreeBoardPostsController(
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
    /// 자유게시판 글 작성 (금지 단어 필터링)
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Create([FromBody] CreatePostRequest request)
    {
        // 금지 단어 체크
        var forbiddenWord = CheckForbiddenWords(request.Title, request.Content);
        if (forbiddenWord != null)
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "FORBIDDEN_WORD",
                    message = $"금지된 단어가 포함되어 있습니다: '{forbiddenWord}'"
                }
            });
        }

        // 자유게시판 카테고리 자동 설정
        request.Category = "free";
        
        return await base.Create(request);
    }

    /// <summary>
    /// 자유게시판 글 수정 (금지 단어 필터링)
    /// </summary>
    [HttpPut("{id:long}")]
    public override async Task<ActionResult<ApiResponse<PostResponse>>> Update(long id, [FromBody] UpdatePostRequest request)
    {
        // 금지 단어 체크
        var content = request.Content ?? "";
        var title = request.Title ?? "";
        var forbiddenWord = CheckForbiddenWords(title, content);
        if (forbiddenWord != null)
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = "FORBIDDEN_WORD",
                    message = $"금지된 단어가 포함되어 있습니다: '{forbiddenWord}'"
                }
            });
        }

        return await base.Update(id, request);
    }

    /// <summary>
    /// 게시판 정보 조회
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetBoardInfo()
    {
        return Ok(new
        {
            name = "자유 게시판",
            description = "회원이라면 누구나 자유롭게 글을 작성할 수 있습니다.",
            rules = new[]
            {
                "광고/홍보 글은 삭제됩니다.",
                "타인을 비방하는 글은 금지됩니다.",
                "정치적 내용은 자제해주세요."
            },
            features = new[] { "글 작성", "댓글", "좋아요", "북마크" }
        });
    }

    /// <summary>
    /// 금지 단어 목록 조회
    /// </summary>
    [HttpGet("forbidden-words")]
    public ActionResult<object> GetForbiddenWords()
    {
        return Ok(new
        {
            words = _forbiddenWords.ToList(),
            description = "게시글에 포함될 수 없는 단어 목록입니다."
        });
    }

    private string? CheckForbiddenWords(string title, string content)
    {
        var combined = $"{title} {content}";
        foreach (var word in _forbiddenWords)
        {
            if (combined.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return word;
            }
        }
        return null;
    }
}
