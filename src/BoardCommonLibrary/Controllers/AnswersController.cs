using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// Q&A 답변 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnswersController : ControllerBase
{
    private readonly IAnswerService _answerService;
    private readonly IQuestionService _questionService;
    private readonly IValidator<CreateAnswerRequest> _createValidator;
    private readonly IValidator<UpdateAnswerRequest> _updateValidator;
    
    public AnswersController(
        IAnswerService answerService,
        IQuestionService questionService,
        IValidator<CreateAnswerRequest> createValidator,
        IValidator<UpdateAnswerRequest> updateValidator)
    {
        _answerService = answerService;
        _questionService = questionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    /// <summary>
    /// 답변 상세 조회
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="currentUserId">현재 사용자 ID</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResponse>>> GetById(
        long id,
        [FromQuery] long? currentUserId = null)
    {
        var answer = await _answerService.GetByIdAsync(id, currentUserId);
        
        if (answer == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<AnswerResponse>.Ok(answer));
    }
    
    /// <summary>
    /// 답변 작성
    /// </summary>
    /// <param name="questionId">질문 ID</param>
    /// <param name="request">답변 작성 요청</param>
    /// <param name="authorId">작성자 ID</param>
    /// <param name="authorName">작성자명</param>
    [HttpPost("/api/questions/{questionId:long}/answers")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResponse>>> Create(
        long questionId,
        [FromBody] CreateAnswerRequest request,
        [FromQuery] long authorId,
        [FromQuery] string authorName = "Anonymous")
    {
        // 질문 존재 확인
        if (!await _questionService.ExistsAsync(questionId))
        {
            return NotFound(ApiErrorResponse.Create(
                "QUESTION_NOT_FOUND",
                "질문을 찾을 수 없습니다."));
        }
        
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
        
        var answer = await _answerService.CreateAsync(questionId, request, authorId, authorName);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = answer.Id },
            ApiResponse<AnswerResponse>.Ok(answer));
    }
    
    /// <summary>
    /// 답변 수정
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="request">답변 수정 요청</param>
    /// <param name="userId">사용자 ID</param>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResponse>>> Update(
        long id,
        [FromBody] UpdateAnswerRequest request,
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
        if (!await _answerService.IsAuthorAsync(id, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "본인의 답변만 수정할 수 있습니다."));
        }
        
        var updatedAnswer = await _answerService.UpdateAsync(id, request, userId);
        
        return Ok(ApiResponse<AnswerResponse>.Ok(updatedAnswer));
    }
    
    /// <summary>
    /// 답변 삭제
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID</param>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(long id, [FromQuery] long userId)
    {
        var answer = await _answerService.GetByIdAsync(id);
        if (answer == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        // 권한 확인
        if (!await _answerService.IsAuthorAsync(id, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "본인의 답변만 삭제할 수 있습니다."));
        }
        
        // 채택된 답변은 삭제 불가
        if (answer.IsAccepted)
        {
            return Conflict(ApiErrorResponse.Create(
                "CANNOT_DELETE",
                "채택된 답변은 삭제할 수 없습니다."));
        }
        
        await _answerService.DeleteAsync(id, userId);
        
        return NoContent();
    }
    
    /// <summary>
    /// 답변 채택
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID (질문 작성자)</param>
    [HttpPost("{id:long}/accept")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResponse>>> Accept(long id, [FromQuery] long userId)
    {
        var answer = await _answerService.GetByIdAsync(id);
        if (answer == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        // 질문 작성자 확인
        if (!await _questionService.IsAuthorAsync(answer.QuestionId, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "질문 작성자만 답변을 채택할 수 있습니다."));
        }
        
        var acceptedAnswer = await _answerService.AcceptAsync(id, userId);
        
        return Ok(ApiResponse<AnswerResponse>.Ok(acceptedAnswer));
    }
    
    /// <summary>
    /// 답변 채택 취소
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID (질문 작성자)</param>
    [HttpDelete("{id:long}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Unaccept(long id, [FromQuery] long userId)
    {
        var answer = await _answerService.GetByIdAsync(id);
        if (answer == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        // 질문 작성자 확인
        if (!await _questionService.IsAuthorAsync(answer.QuestionId, userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Create(
                "FORBIDDEN",
                "질문 작성자만 채택을 취소할 수 있습니다."));
        }
        
        await _answerService.UnacceptAsync(id, userId);
        
        return NoContent();
    }
    
    /// <summary>
    /// 답변 추천/비추천
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="request">추천 요청</param>
    /// <param name="userId">사용자 ID</param>
    [HttpPost("{id:long}/vote")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResponse>>> Vote(
        long id,
        [FromBody] VoteRequest request,
        [FromQuery] long userId)
    {
        if (!await _answerService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        var updatedAnswer = await _answerService.VoteAsync(id, userId, request.VoteType);
        
        return Ok(ApiResponse<AnswerResponse>.Ok(updatedAnswer));
    }
    
    /// <summary>
    /// 답변 추천 취소
    /// </summary>
    /// <param name="id">답변 ID</param>
    /// <param name="userId">사용자 ID</param>
    [HttpDelete("{id:long}/vote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelVote(long id, [FromQuery] long userId)
    {
        if (!await _answerService.ExistsAsync(id))
        {
            return NotFound(ApiErrorResponse.Create(
                "ANSWER_NOT_FOUND",
                "답변을 찾을 수 없습니다."));
        }
        
        await _answerService.RemoveVoteAsync(id, userId);
        
        return NoContent();
    }
}
