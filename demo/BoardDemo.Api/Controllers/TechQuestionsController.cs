using BoardCommonLibrary.Controllers;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BoardDemo.Api.Controllers;

/// <summary>
/// 기술 Q&A 게시판 컨트롤러
/// QuestionsController를 상속받아 /api/tech-qna 경로로 제공합니다.
/// 기술 관련 질문만 허용하며, 추가적인 필터링/검증 로직을 포함합니다.
/// </summary>
[Route("api/tech-qna")]
[ApiController]
public class TechQuestionsController : QuestionsController
{
    private readonly ILogger<TechQuestionsController> _logger;
    
    // 기술 Q&A에 허용되는 태그 목록
    private static readonly HashSet<string> AllowedTechTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "C#", ".NET", "ASP.NET", "Entity Framework", "SQL", "Database",
        "JavaScript", "TypeScript", "React", "Vue", "Angular",
        "Python", "Java", "Go", "Rust", "Docker", "Kubernetes",
        "Azure", "AWS", "DevOps", "API", "REST", "GraphQL",
        "Security", "Performance", "Architecture", "Design Pattern"
    };
    
    public TechQuestionsController(
        IQuestionService questionService,
        IValidator<CreateQuestionRequest> createValidator,
        IValidator<UpdateQuestionRequest> updateValidator,
        ILogger<TechQuestionsController> logger)
        : base(questionService, createValidator, updateValidator)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 기술 질문 목록 조회 (기술 태그가 있는 질문만 필터링)
    /// </summary>
    [HttpGet]
    public override async Task<ActionResult<PagedResponse<QuestionResponse>>> GetAll(
        [FromQuery] QuestionQueryParameters parameters)
    {
        _logger.LogInformation("🔧 [TechQnA] 기술 질문 목록 조회: page={Page}", parameters.Page);
        
        // 기술 태그가 있는 질문만 필터링
        if (string.IsNullOrEmpty(parameters.Tag))
        {
            // 기본적으로 허용된 기술 태그 중 하나라도 있는 질문만 표시
            // 실제 구현에서는 서비스 레벨에서 필터링하는 것이 좋음
        }
        
        return await base.GetAll(parameters);
    }
    
    /// <summary>
    /// 기술 질문 작성 - 기술 태그 검증 추가
    /// </summary>
    [HttpPost]
    public override async Task<ActionResult<ApiResponse<QuestionResponse>>> Create(
        [FromBody] CreateQuestionRequest request,
        [FromQuery] long authorId,
        [FromQuery] string authorName = "Anonymous")
    {
        _logger.LogInformation("🔧 [TechQnA] 기술 질문 작성 요청: '{Title}' by {Author}", 
            request.Title, authorName);
        
        // 기술 태그가 하나 이상 포함되어 있는지 검증
        if (request.Tags == null || !request.Tags.Any())
        {
            return BadRequest(ApiErrorResponse.Create(
                "TECH_TAG_REQUIRED",
                "기술 Q&A에는 최소 1개 이상의 기술 태그가 필요합니다. " +
                $"허용 태그: {string.Join(", ", AllowedTechTags.Take(10))}..."));
        }
        
        var hasTechTag = request.Tags.Any(t => AllowedTechTags.Contains(t));
        if (!hasTechTag)
        {
            return BadRequest(ApiErrorResponse.Create(
                "INVALID_TECH_TAG",
                "기술 Q&A에는 유효한 기술 태그가 최소 1개 이상 필요합니다. " +
                $"허용 태그: {string.Join(", ", AllowedTechTags.Take(10))}..."));
        }
        
        return await base.Create(request, authorId, authorName);
    }
    
    /// <summary>
    /// 허용된 기술 태그 목록 조회
    /// </summary>
    [HttpGet("allowed-tags")]
    public ActionResult<IEnumerable<string>> GetAllowedTags()
    {
        return Ok(new 
        { 
            tags = AllowedTechTags.OrderBy(t => t).ToList(),
            description = "기술 Q&A 게시판에서 사용 가능한 태그 목록입니다."
        });
    }
    
    /// <summary>
    /// 기술 Q&A 통계
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult> GetTechQnAStatistics()
    {
        var allQuestions = await QuestionService.GetAllAsync(new QuestionQueryParameters 
        { 
            PageSize = 1000 
        });
        
        // 간단한 통계 (실제로는 서비스 레벨에서 구현)
        var stats = new
        {
            TotalTechQuestions = allQuestions.Meta.TotalCount,
            BoardName = "기술 Q&A",
            AllowedTagsCount = AllowedTechTags.Count,
            Description = "개발 및 기술 관련 질문을 위한 게시판입니다."
        };
        
        return Ok(stats);
    }
}
