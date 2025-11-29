using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// Q&A 질문 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IValidator<CreateQuestionRequest> _createValidator;
    private readonly IValidator<UpdateQuestionRequest> _updateValidator;
    
    public QuestionsController(
        IQuestionService questionService,
        IValidator<CreateQuestionRequest> createValidator,
        IValidator<UpdateQuestionRequest> updateValidator)
    {
        _questionService = questionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    /// <summary>
    /// 질문 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<QuestionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<QuestionResponse>>> GetAll(
        [FromQuery] QuestionQueryParameters parameters)
    {
        var result = await _questionService.GetAllAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 질문 상세 조회
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="currentUserId">현재 사용자 ID (투표 상태 확인용)</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<QuestionDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuestionDetailResponse>>> GetById(
        long id,
        [FromQuery] long? currentUserId = null)
    {
        var question = await _questionService.GetByIdAsync(id, currentUserId);
        
        if (question == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        // 조회수 증가
        await _questionService.IncrementViewCountAsync(id);
        
        return Ok(ApiResponse<QuestionDetailResponse>.Ok(question));
    }
    
    /// <summary>
    /// 질문 작성
    /// </summary>
    /// <param name="request">질문 작성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QuestionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QuestionResponse>>> Create(
        [FromBody] CreateQuestionRequest request,
        [FromQuery] long authorId,
        [FromQuery] string authorName = "Anonymous")
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create(
                "VALIDATION_ERROR",
                "입력 데이터가 유효하지 않습니다.",
                validationResult.Errors.Select(e => new ValidationError
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));
        }
        
        var question = await _questionService.CreateAsync(request, authorId, authorName);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = question.Id },
            ApiResponse<QuestionResponse>.Ok(question));
    }
    
    /// <summary>
    /// 질문 수정
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="request">질문 수정 요청</param>
    /// <param name="userId">사용자 ID</param>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuestionResponse>>> Update(
        long id,
        [FromBody] UpdateQuestionRequest request,
        [FromQuery] long userId)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create(
                "VALIDATION_ERROR",
                "입력 데이터가 유효하지 않습니다.",
                validationResult.Errors.Select(e => new ValidationError
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));
        }
        
        // 권한 확인
        if (!await _questionService.IsAuthorAsync(id, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "본인의 질문만 수정할 수 있습니다."));
        }
        
        var updatedQuestion = await _questionService.UpdateAsync(id, request, userId);
        
        return Ok(ApiResponse<QuestionResponse>.Ok(updatedQuestion));
    }
    
    /// <summary>
    /// 질문 삭제
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">사용자 ID</param>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(long id, [FromQuery] long userId)
    {
        if (!await _questionService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        if (!await _questionService.IsAuthorAsync(id, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "본인의 질문만 삭제할 수 있습니다."));
        }
        
        var result = await _questionService.DeleteAsync(id, userId);
        
        if (!result)
        {
            return Conflict(ApiErrorResponse.Create(
                "CANNOT_DELETE",
                "답변이 있는 질문은 삭제할 수 없습니다."));
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// 질문 추천/비추천
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="request">추천 요청</param>
    /// <param name="userId">사용자 ID</param>
    [HttpPost("{id:long}/vote")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Vote(
        long id,
        [FromBody] VoteRequest request,
        [FromQuery] long userId)
    {
        if (!await _questionService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        var newVoteCount = await _questionService.VoteAsync(id, userId, request.VoteType);
        
        return Ok(ApiResponse<object>.Ok(new { voteCount = newVoteCount }));
    }
    
    /// <summary>
    /// 질문 추천 취소
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">사용자 ID</param>
    [HttpDelete("{id:long}/vote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelVote(long id, [FromQuery] long userId)
    {
        if (!await _questionService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        await _questionService.RemoveVoteAsync(id, userId);
        
        return NoContent();
    }
    
    /// <summary>
    /// 질문 종료
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="userId">사용자 ID (질문 작성자)</param>
    [HttpPost("{id:long}/close")]
    [ProducesResponseType(typeof(ApiResponse<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuestionResponse>>> Close(long id, [FromQuery] long userId)
    {
        if (!await _questionService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        if (!await _questionService.IsAuthorAsync(id, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "본인의 질문만 종료할 수 있습니다."));
        }
        
        var closedQuestion = await _questionService.CloseAsync(id, userId);
        
        return Ok(ApiResponse<QuestionResponse>.Ok(closedQuestion));
    }
    
    /// <summary>
    /// 질문의 답변 목록 조회
    /// </summary>
    /// <param name="id">질문 ID</param>
    /// <param name="currentUserId">현재 사용자 ID</param>
    [HttpGet("{id:long}/answers")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AnswerResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AnswerResponse>>>> GetAnswers(
        long id,
        [FromQuery] long? currentUserId = null)
    {
        var question = await _questionService.GetByIdAsync(id, currentUserId);
        
        if (question == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<IEnumerable<AnswerResponse>>.Ok(question.Answers));
    }
}
