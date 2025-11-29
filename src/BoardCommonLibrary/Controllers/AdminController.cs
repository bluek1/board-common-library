using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 관리자 API 컨트롤러
/// 상속하여 커스터마이징 가능합니다.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    protected readonly IAdminService AdminService;
    protected readonly IReportService ReportService;
    protected readonly IValidator<ProcessReportRequest> ProcessReportValidator;
    
    public AdminController(
        IAdminService adminService,
        IReportService reportService,
        IValidator<ProcessReportRequest> processReportValidator)
    {
        AdminService = adminService;
        ReportService = reportService;
        ProcessReportValidator = processReportValidator;
    }
    
    /// <summary>
    /// 전체 게시물 관리 조회
    /// </summary>
    /// <remarks>
    /// 삭제된 게시물, 블라인드 처리된 게시물을 포함한 전체 게시물 목록을 조회합니다.
    /// </remarks>
    [HttpGet("posts")]
    [ProducesResponseType(typeof(PagedResponse<PostResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<PagedResponse<PostResponse>>> GetAllPosts(
        [FromQuery] AdminPostQueryParameters parameters)
    {
        var result = await AdminService.GetAllPostsAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 전체 댓글 관리 조회
    /// </summary>
    /// <remarks>
    /// 삭제된 댓글, 블라인드 처리된 댓글을 포함한 전체 댓글 목록을 조회합니다.
    /// </remarks>
    [HttpGet("comments")]
    [ProducesResponseType(typeof(PagedResponse<CommentResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<PagedResponse<CommentResponse>>> GetAllComments(
        [FromQuery] AdminCommentQueryParameters parameters)
    {
        var result = await AdminService.GetAllCommentsAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 신고 목록 조회
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(PagedResponse<ReportResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<PagedResponse<ReportResponse>>> GetReports(
        [FromQuery] ReportQueryParameters parameters)
    {
        var result = await ReportService.GetAllAsync(parameters);
        return Ok(result);
    }
    
    /// <summary>
    /// 신고 상세 조회
    /// </summary>
    /// <param name="id">신고 ID</param>
    [HttpGet("reports/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<ReportResponse>>> GetReportById(long id)
    {
        var report = await ReportService.GetByIdAsync(id);
        
        if (report == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "REPORT_NOT_FOUND",
                "신고 내역을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<ReportResponse>.Ok(report));
    }
    
    /// <summary>
    /// 신고 처리
    /// </summary>
    /// <param name="id">신고 ID</param>
    /// <param name="request">처리 요청</param>
    /// <param name="processedById">처리자 ID</param>
    /// <param name="processedByName">처리자명</param>
    [HttpPut("reports/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<ReportResponse>>> ProcessReport(
        long id,
        [FromBody] ProcessReportRequest request,
        [FromQuery] long processedById,
        [FromQuery] string processedByName = "Admin")
    {
        var validationResult = await ProcessReportValidator.ValidateAsync(request);
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
        
        var report = await ReportService.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "REPORT_NOT_FOUND",
                "신고 내역을 찾을 수 없습니다."));
        }
        
        var processedReport = await ReportService.ProcessAsync(id, request, processedById, processedByName);
        
        return Ok(ApiResponse<ReportResponse>.Ok(processedReport));
    }
    
    /// <summary>
    /// 콘텐츠 블라인드 처리
    /// </summary>
    /// <param name="request">블라인드 요청</param>
    [HttpPost("blind")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public virtual async Task<ActionResult<ApiResponse<object>>> BlindContent(
        [FromBody] BlindContentRequest request)
    {
        if (request.TargetType != BatchTargetType.Post && request.TargetType != BatchTargetType.Comment)
        {
            return BadRequest(ApiErrorResponse.Create(
                "INVALID_TARGET_TYPE",
                "게시물 또는 댓글만 블라인드 처리할 수 있습니다."));
        }
        
        var result = await AdminService.BlindContentAsync(request);
        
        if (!result)
        {
            return NotFound(ApiErrorResponse.Create(
                "TARGET_NOT_FOUND",
                "대상 콘텐츠를 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<object>.Ok(new { blinded = request.IsBlinded }));
    }
    
    /// <summary>
    /// 일괄 삭제
    /// </summary>
    /// <param name="request">일괄 삭제 요청</param>
    [HttpPost("batch/delete")]
    [ProducesResponseType(typeof(ApiResponse<BatchDeleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public virtual async Task<ActionResult<ApiResponse<BatchDeleteResponse>>> BatchDelete(
        [FromBody] BatchDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(ApiErrorResponse.Create(
                "INVALID_REQUEST",
                "삭제할 대상을 선택해주세요."));
        }
        
        var result = await AdminService.BatchDeleteAsync(request);
        
        return Ok(ApiResponse<BatchDeleteResponse>.Ok(result));
    }
    
    /// <summary>
    /// 통계 조회
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<BoardStatisticsResponse>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<ApiResponse<BoardStatisticsResponse>>> GetStatistics()
    {
        var statistics = await AdminService.GetStatisticsAsync();
        
        return Ok(ApiResponse<BoardStatisticsResponse>.Ok(statistics));
    }
    
    /// <summary>
    /// 대기 중인 신고 수 조회
    /// </summary>
    [HttpGet("reports/pending/count")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public virtual async Task<ActionResult<ApiResponse<object>>> GetPendingReportCount()
    {
        var count = await ReportService.GetPendingCountAsync();
        return Ok(ApiResponse<object>.Ok(new { pendingCount = count }));
    }
}
