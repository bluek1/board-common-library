using Microsoft.AspNetCore.Mvc;
using BoardCommonLibrary.Interfaces;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 4 테스트 컨트롤러: 관리자 기능 및 Q&A 게시판
/// </summary>
[ApiController]
[Route("api/test/page4")]
public class TestPage4Controller : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IAnswerService _answerService;
    private readonly IReportService _reportService;
    private readonly IAdminService _adminService;

    public TestPage4Controller(
        IQuestionService questionService,
        IAnswerService answerService,
        IReportService reportService,
        IAdminService adminService)
    {
        _questionService = questionService;
        _answerService = answerService;
        _reportService = reportService;
        _adminService = adminService;
    }

    #region Admin - 게시물 관리 (P4-001)

    /// <summary>
    /// 관리자용 전체 게시물 조회
    /// </summary>
    [HttpGet("admin/posts")]
    public async Task<ActionResult<PagedResponse<PostResponse>>> GetAllPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PostStatus? status = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isBlinded = null)
    {
        var parameters = new AdminPostQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            IsDeleted = isDeleted,
            IsBlinded = isBlinded
        };
        
        var result = await _adminService.GetAllPostsAsync(parameters);
        return Ok(result);
    }

    #endregion

    #region Admin - 댓글 관리 (P4-002)

    /// <summary>
    /// 관리자용 전체 댓글 조회
    /// </summary>
    [HttpGet("admin/comments")]
    public async Task<ActionResult<PagedResponse<CommentResponse>>> GetAllComments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isBlinded = null)
    {
        var parameters = new AdminCommentQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            IsDeleted = isDeleted,
            IsBlinded = isBlinded
        };
        
        var result = await _adminService.GetAllCommentsAsync(parameters);
        return Ok(result);
    }

    #endregion

    #region Admin - 신고 관리 (P4-003, P4-004)

    /// <summary>
    /// 신고 목록 조회
    /// </summary>
    [HttpGet("admin/reports")]
    public async Task<ActionResult<PagedResponse<ReportResponse>>> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ReportStatus? status = null,
        [FromQuery] ReportTargetType? targetType = null)
    {
        var parameters = new ReportQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            TargetType = targetType
        };
        
        var result = await _reportService.GetAllAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// 신고 상세 조회
    /// </summary>
    [HttpGet("admin/reports/{id:long}")]
    public async Task<ActionResult<ReportResponse>> GetReport(long id)
    {
        var result = await _reportService.GetByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "신고를 찾을 수 없습니다." });
        return Ok(result);
    }

    /// <summary>
    /// 신고 처리
    /// </summary>
    [HttpPut("admin/reports/{id:long}")]
    public async Task<ActionResult<ReportResponse>> ProcessReport(long id, [FromBody] ProcessReportApiRequest request)
    {
        try
        {
            var processRequest = new ProcessReportRequest
            {
                Status = request.Status,
                ProcessingNote = request.ProcessingNote
            };
            
            var result = await _reportService.ProcessAsync(id, processRequest, request.ProcessorId, request.ProcessorName ?? "Admin");
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "신고를 찾을 수 없습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 신고 생성
    /// </summary>
    [HttpPost("reports")]
    public async Task<ActionResult<ReportResponse>> CreateReport([FromBody] CreateReportApiRequest request)
    {
        try
        {
            var createRequest = new CreateReportRequest
            {
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                Reason = request.Reason,
                Description = request.Description
            };
            
            var result = await _reportService.CreateAsync(createRequest, request.ReporterId, request.ReporterName ?? "User");
            return CreatedAtAction(nameof(GetReport), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    #endregion

    #region Admin - 콘텐츠 블라인드 (P4-005)

    /// <summary>
    /// 콘텐츠 블라인드 처리
    /// </summary>
    [HttpPost("admin/blind")]
    public async Task<ActionResult> BlindContent([FromBody] BlindContentApiRequest request)
    {
        try
        {
            var blindRequest = new BlindContentRequest
            {
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                IsBlinded = request.IsBlinded,
                Reason = request.Reason
            };
            
            var result = await _adminService.BlindContentAsync(blindRequest);
            return Ok(new { message = request.IsBlinded ? "블라인드 처리되었습니다." : "블라인드가 해제되었습니다.", success = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "콘텐츠를 찾을 수 없습니다." });
        }
    }

    #endregion

    #region Admin - 일괄 삭제 (P4-006)

    /// <summary>
    /// 일괄 삭제
    /// </summary>
    [HttpPost("admin/batch/delete")]
    public async Task<ActionResult<BatchDeleteResponse>> BatchDelete([FromBody] BatchDeleteApiRequest request)
    {
        var deleteRequest = new BatchDeleteRequest
        {
            TargetType = request.TargetType,
            Ids = request.Ids,
            HardDelete = request.HardDelete
        };
        
        var result = await _adminService.BatchDeleteAsync(deleteRequest);
        return Ok(result);
    }

    #endregion

    #region Admin - 통계 조회 (P4-007)

    /// <summary>
    /// 게시판 통계 조회
    /// </summary>
    [HttpGet("admin/statistics")]
    public async Task<ActionResult<BoardStatisticsResponse>> GetStatistics()
    {
        var result = await _adminService.GetStatisticsAsync();
        return Ok(result);
    }

    #endregion

    #region Q&A - 질문 관리 (P4-008, P4-009)

    /// <summary>
    /// 질문 목록 조회
    /// </summary>
    [HttpGet("questions")]
    public async Task<ActionResult<PagedResponse<QuestionResponse>>> GetQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] QuestionStatus? status = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? sort = "createdAt",
        [FromQuery] string? order = "desc")
    {
        var parameters = new QuestionQueryParameters
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Tag = tag,
            Sort = sort ?? "createdAt",
            Order = order ?? "desc"
        };
        
        var result = await _questionService.GetAllAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// 질문 상세 조회
    /// </summary>
    [HttpGet("questions/{id:long}")]
    public async Task<ActionResult<QuestionDetailResponse>> GetQuestion(long id, [FromQuery] long? currentUserId = null)
    {
        var result = await _questionService.GetByIdAsync(id, currentUserId);
        if (result == null)
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        return Ok(result);
    }

    /// <summary>
    /// 질문 작성
    /// </summary>
    [HttpPost("questions")]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionApiRequest request)
    {
        var createRequest = new CreateQuestionRequest
        {
            Title = request.Title,
            Content = request.Content,
            Tags = request.Tags,
            BountyPoints = request.BountyPoints
        };
        
        var result = await _questionService.CreateAsync(createRequest, request.AuthorId, request.AuthorName ?? "User");
        return CreatedAtAction(nameof(GetQuestion), new { id = result.Id }, result);
    }

    /// <summary>
    /// 질문 수정
    /// </summary>
    [HttpPut("questions/{id:long}")]
    public async Task<ActionResult<QuestionResponse>> UpdateQuestion(long id, [FromBody] UpdateQuestionApiRequest request)
    {
        try
        {
            var updateRequest = new UpdateQuestionRequest
            {
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags
            };
            
            var result = await _questionService.UpdateAsync(id, updateRequest, request.UserId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 질문 삭제
    /// </summary>
    [HttpDelete("questions/{id:long}")]
    public async Task<ActionResult> DeleteQuestion(long id, [FromQuery] long userId)
    {
        try
        {
            var result = await _questionService.DeleteAsync(id, userId);
            if (!result)
                return BadRequest(new { message = "질문을 삭제할 수 없습니다." });
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 질문 종료
    /// </summary>
    [HttpPost("questions/{id:long}/close")]
    public async Task<ActionResult<QuestionResponse>> CloseQuestion(long id, [FromBody] QuestionUserRequest request)
    {
        try
        {
            var result = await _questionService.CloseAsync(id, request.UserId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 질문 재개
    /// </summary>
    [HttpPost("questions/{id:long}/reopen")]
    public async Task<ActionResult<QuestionResponse>> ReopenQuestion(long id, [FromBody] QuestionUserRequest request)
    {
        try
        {
            var result = await _questionService.ReopenAsync(id, request.UserId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 질문 추천
    /// </summary>
    [HttpPost("questions/{id:long}/vote")]
    public async Task<ActionResult> VoteQuestion(long id, [FromBody] VoteApiRequest request)
    {
        try
        {
            var newVoteCount = await _questionService.VoteAsync(id, request.UserId, request.VoteType);
            return Ok(new { voteCount = newVoteCount });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 질문 추천 취소
    /// </summary>
    [HttpDelete("questions/{id:long}/vote")]
    public async Task<ActionResult> RemoveVoteQuestion(long id, [FromQuery] long userId)
    {
        try
        {
            var result = await _questionService.RemoveVoteAsync(id, userId);
            if (!result)
                return NotFound(new { message = "투표를 찾을 수 없습니다." });
            return Ok(new { message = "투표가 취소되었습니다." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
    }

    /// <summary>
    /// 조회수 증가
    /// </summary>
    [HttpPost("questions/{id:long}/view")]
    public async Task<ActionResult> IncrementViewCount(long id)
    {
        try
        {
            await _questionService.IncrementViewCountAsync(id);
            return Ok(new { message = "조회수가 증가되었습니다." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
    }

    #endregion

    #region Q&A - 답변 관리 (P4-010, P4-011, P4-012)

    /// <summary>
    /// 질문의 답변 목록 조회
    /// </summary>
    [HttpGet("questions/{questionId:long}/answers")]
    public async Task<ActionResult<List<AnswerResponse>>> GetAnswers(
        long questionId,
        [FromQuery] long? currentUserId = null)
    {
        var result = await _answerService.GetByQuestionIdAsync(questionId, currentUserId);
        return Ok(result);
    }

    /// <summary>
    /// 답변 상세 조회
    /// </summary>
    [HttpGet("answers/{id:long}")]
    public async Task<ActionResult<AnswerResponse>> GetAnswer(long id, [FromQuery] long? currentUserId = null)
    {
        var result = await _answerService.GetByIdAsync(id, currentUserId);
        if (result == null)
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        return Ok(result);
    }

    /// <summary>
    /// 답변 작성
    /// </summary>
    [HttpPost("questions/{questionId:long}/answers")]
    public async Task<ActionResult<AnswerResponse>> CreateAnswer(long questionId, [FromBody] CreateAnswerApiRequest request)
    {
        try
        {
            var createRequest = new CreateAnswerRequest
            {
                Content = request.Content
            };
            
            var result = await _answerService.CreateAsync(questionId, createRequest, request.AuthorId, request.AuthorName ?? "User");
            return CreatedAtAction(nameof(GetAnswer), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "질문을 찾을 수 없습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 답변 수정
    /// </summary>
    [HttpPut("answers/{id:long}")]
    public async Task<ActionResult<AnswerResponse>> UpdateAnswer(long id, [FromBody] UpdateAnswerApiRequest request)
    {
        try
        {
            var updateRequest = new UpdateAnswerRequest
            {
                Content = request.Content
            };
            
            var result = await _answerService.UpdateAsync(id, updateRequest, request.UserId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 답변 삭제
    /// </summary>
    [HttpDelete("answers/{id:long}")]
    public async Task<ActionResult> DeleteAnswer(long id, [FromQuery] long userId)
    {
        try
        {
            var result = await _answerService.DeleteAsync(id, userId);
            if (!result)
                return BadRequest(new { message = "답변을 삭제할 수 없습니다." });
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 답변 채택
    /// </summary>
    [HttpPost("answers/{id:long}/accept")]
    public async Task<ActionResult<AnswerResponse>> AcceptAnswer(long id, [FromBody] AcceptAnswerApiRequest request)
    {
        try
        {
            var result = await _answerService.AcceptAsync(id, request.QuestionAuthorId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 답변 채택 취소
    /// </summary>
    [HttpDelete("answers/{id:long}/accept")]
    public async Task<ActionResult> UnacceptAnswer(long id, [FromBody] UnacceptAnswerApiRequest request)
    {
        try
        {
            var result = await _answerService.UnacceptAsync(id, request.QuestionAuthorId);
            if (!result)
                return BadRequest(new { message = "채택 취소에 실패했습니다." });
            return Ok(new { message = "채택이 취소되었습니다." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 답변 추천
    /// </summary>
    [HttpPost("answers/{id:long}/vote")]
    public async Task<ActionResult<AnswerResponse>> VoteAnswer(long id, [FromBody] VoteApiRequest request)
    {
        try
        {
            var result = await _answerService.VoteAsync(id, request.UserId, request.VoteType);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 답변 추천 취소
    /// </summary>
    [HttpDelete("answers/{id:long}/vote")]
    public async Task<ActionResult> RemoveVoteAnswer(long id, [FromQuery] long userId)
    {
        try
        {
            var result = await _answerService.RemoveVoteAsync(id, userId);
            if (!result)
                return NotFound(new { message = "투표를 찾을 수 없습니다." });
            return Ok(new { message = "투표가 취소되었습니다." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "답변을 찾을 수 없습니다." });
        }
    }

    #endregion
}

#region API Request Models

/// <summary>
/// 신고 처리 API 요청
/// </summary>
public class ProcessReportApiRequest
{
    public ReportStatus Status { get; set; }
    public string? ProcessingNote { get; set; }
    public long ProcessorId { get; set; }
    public string? ProcessorName { get; set; }
}

/// <summary>
/// 신고 생성 API 요청
/// </summary>
public class CreateReportApiRequest
{
    public ReportTargetType TargetType { get; set; }
    public long TargetId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public long ReporterId { get; set; }
    public string? ReporterName { get; set; }
}

/// <summary>
/// 블라인드 처리 API 요청
/// </summary>
public class BlindContentApiRequest
{
    public BatchTargetType TargetType { get; set; }
    public long TargetId { get; set; }
    public bool IsBlinded { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// 일괄 삭제 API 요청
/// </summary>
public class BatchDeleteApiRequest
{
    public BatchTargetType TargetType { get; set; }
    public List<long> Ids { get; set; } = new();
    public bool HardDelete { get; set; } = false;
}

/// <summary>
/// 질문 생성 API 요청
/// </summary>
public class CreateQuestionApiRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public int BountyPoints { get; set; }
    public long AuthorId { get; set; }
    public string? AuthorName { get; set; }
}

/// <summary>
/// 질문 수정 API 요청
/// </summary>
public class UpdateQuestionApiRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public long UserId { get; set; }
}

/// <summary>
/// 질문 사용자 요청 (종료/재개)
/// </summary>
public class QuestionUserRequest
{
    public long UserId { get; set; }
}

/// <summary>
/// 투표 API 요청
/// </summary>
public class VoteApiRequest
{
    public long UserId { get; set; }
    public VoteType VoteType { get; set; }
}

/// <summary>
/// 답변 생성 API 요청
/// </summary>
public class CreateAnswerApiRequest
{
    public string Content { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public string? AuthorName { get; set; }
}

/// <summary>
/// 답변 수정 API 요청
/// </summary>
public class UpdateAnswerApiRequest
{
    public string Content { get; set; } = string.Empty;
    public long UserId { get; set; }
}

/// <summary>
/// 답변 채택 API 요청
/// </summary>
public class AcceptAnswerApiRequest
{
    public long QuestionAuthorId { get; set; }
}

/// <summary>
/// 답변 채택 취소 API 요청
/// </summary>
public class UnacceptAnswerApiRequest
{
    public long QuestionAuthorId { get; set; }
}

#endregion
