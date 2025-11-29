using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardCommonLibrary.Controllers;

/// <summary>
/// 신고 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IValidator<CreateReportRequest> _createValidator;
    
    public ReportsController(
        IReportService reportService,
        IValidator<CreateReportRequest> createValidator)
    {
        _reportService = reportService;
        _createValidator = createValidator;
    }
    
    /// <summary>
    /// 신고하기
    /// </summary>
    /// <param name="request">신고 요청</param>
    /// <param name="reporterId">신고자 ID</param>
    /// <param name="reporterName">신고자명</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ReportResponse>>> Create(
        [FromBody] CreateReportRequest request,
        [FromQuery] long reporterId,
        [FromQuery] string reporterName = "Anonymous")
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
        
        // 중복 신고 확인
        if (await _reportService.HasReportedAsync(request.TargetType, request.TargetId, reporterId))
        {
            return Conflict(ApiErrorResponse.Create(
                "ALREADY_REPORTED",
                "이미 신고한 콘텐츠입니다."));
        }
        
        // 대상 존재 확인
        if (!await _reportService.TargetExistsAsync(request.TargetType, request.TargetId))
        {
            return NotFound(ApiErrorResponse.Create(
                "TARGET_NOT_FOUND",
                "신고 대상을 찾을 수 없습니다."));
        }
        
        var report = await _reportService.CreateAsync(request, reporterId, reporterName);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = report.Id },
            ApiResponse<ReportResponse>.Ok(report));
    }
    
    /// <summary>
    /// 신고 상세 조회
    /// </summary>
    /// <param name="id">신고 ID</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReportResponse>>> GetById(long id)
    {
        var report = await _reportService.GetByIdAsync(id);
        
        if (report == null)
        {
            return NotFound(ApiErrorResponse.Create(
                "REPORT_NOT_FOUND",
                "신고 내역을 찾을 수 없습니다."));
        }
        
        return Ok(ApiResponse<ReportResponse>.Ok(report));
    }
    
    /// <summary>
    /// 대상별 신고 횟수 조회
    /// </summary>
    /// <param name="targetType">대상 유형</param>
    /// <param name="targetId">대상 ID</param>
    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetReportCount(
        [FromQuery] ReportTargetType targetType,
        [FromQuery] long targetId)
    {
        var count = await _reportService.GetReportCountAsync(targetType, targetId);
        return Ok(ApiResponse<object>.Ok(new { targetType, targetId, count }));
    }
}
