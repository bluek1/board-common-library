using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 일반 Q&A 게시판 컨트롤러
/// QuestionsController를 상속받아 /api/general-qna 경로로 제공합니다.
/// 일상, 취미, 기타 일반적인 질문을 위한 게시판입니다.
/// </summary>
[Route("api/general-qna")]
[ApiController]
public class GeneralQuestionsController : QuestionsController
{
    private readonly ILogger<GeneralQuestionsController> _logger;
    
    // 일반 Q&A에 허용되는 태그 목록
    private static readonly HashSet<string> AllowedGeneralTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "일상", "취미", "여행", "음식", "건강", "운동",
        "독서", "영화", "음악", "게임", "반려동물",
        "재테크", "부동산", "자동차", "패션", "뷰티",
        "육아", "교육", "진로", "취업", "창업",
        "기타", "질문", "도움요청", "추천", "의견"
    };
    
    public GeneralQuestionsController(
        IQuestionService questionService,
        IValidator<CreateQuestionRequest> createValidator,
        IValidator<UpdateQuestionRequest> updateValidator,
        ILogger<GeneralQuestionsController> logger)
        : base(questionService, createValidator, updateValidator)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 일반 질문 목록 조회
    /// </summary>
    [HttpGet]
    public override async Task<ActionResult<PagedResponse<QuestionResponse>>> GetAll(
        [FromQuery] QuestionQueryParameters parameters)
    {
        _logger.LogInformation("💬 [GeneralQnA] 일반 질문 목록 조회: page={Page}", parameters.Page);
        
        return await base.GetAll(parameters);
    }
    
    /// <summary>
    /// 일반 질문 작성
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<QuestionResponse>>> Create(
        [FromBody] CreateQuestionRequest request,
        [FromQuery] long authorId,
        [FromQuery] string authorName = "Anonymous")
    {
        _logger.LogInformation("💬 [GeneralQnA] 일반 질문 작성 요청: '{Title}' by {Author}", 
            request.Title, authorName);
        
        // 기술 태그가 포함되어 있으면 경고 (단, 허용은 함)
        var techTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C#", ".NET", "JavaScript", "Python", "Java", "SQL", "Docker"
        };
        
        if (request.Tags != null && request.Tags.Any(t => techTags.Contains(t)))
        {
            _logger.LogWarning(
                "💬 [GeneralQnA] 기술 태그가 포함된 질문입니다. 기술 Q&A 게시판 이용을 권장합니다. " +
                "질문: '{Title}'", request.Title);
        }
        
        return await base.Create(request, authorId, authorName);
    }
    
    /// <summary>
    /// 허용된 일반 태그 목록 조회
    /// </summary>
    [HttpGet("allowed-tags")]
    public ActionResult<IEnumerable<string>> GetAllowedTags()
    {
        return Ok(new 
        { 
            tags = AllowedGeneralTags.OrderBy(t => t).ToList(),
            description = "일반 Q&A 게시판에서 사용 가능한 태그 목록입니다."
        });
    }
    
    /// <summary>
    /// 일반 Q&A 통계
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult> GetGeneralQnAStatistics()
    {
        var allQuestions = await QuestionService.GetAllAsync(new QuestionQueryParameters 
        { 
            PageSize = 1000 
        });
        
        var stats = new
        {
            TotalGeneralQuestions = allQuestions.Meta.TotalCount,
            BoardName = "일반 Q&A",
            AllowedTagsCount = AllowedGeneralTags.Count,
            Description = "일상생활, 취미, 기타 일반적인 질문을 위한 게시판입니다."
        };
        
        return Ok(stats);
    }
    
    /// <summary>
    /// 인기 질문 조회 (조회수 + 추천수 기준)
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<PagedResponse<QuestionResponse>>> GetPopularQuestions(
        [FromQuery] int count = 10)
    {
        _logger.LogInformation("💬 [GeneralQnA] 인기 질문 조회: count={Count}", count);
        
        // 인기 질문 조회 (기본 정렬을 추천수로)
        var parameters = new QuestionQueryParameters
        {
            Page = 1,
            PageSize = count,
            Sort = "voteCount",
            Order = "desc"
        };
        
        return await base.GetAll(parameters);
    }
}
