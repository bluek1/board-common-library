using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 신고 서비스 구현
/// </summary>
public class ReportService : IReportService
{
    private readonly BoardDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(BoardDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region 신고 생성

    /// <inheritdoc />
    public async Task<ReportResponse> CreateAsync(CreateReportRequest request, long reporterId, string reporterName)
    {
        // 대상 존재 여부 확인
        if (!await TargetExistsAsync(request.TargetType, request.TargetId))
            throw new KeyNotFoundException($"신고 대상을 찾을 수 없습니다: {request.TargetType} {request.TargetId}");

        // 중복 신고 확인
        if (await HasReportedAsync(request.TargetType, request.TargetId, reporterId))
            throw new InvalidOperationException("이미 신고한 콘텐츠입니다.");

        var report = new Report
        {
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            ReporterId = reporterId,
            ReporterName = reporterName,
            Reason = request.Reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("신고 생성됨: {ReportId}, 대상: {TargetType} {TargetId}, 신고자: {ReporterId}", 
            report.Id, request.TargetType, request.TargetId, reporterId);

        return await GetReportResponseAsync(report);
    }

    #endregion

    #region 신고 조회

    /// <inheritdoc />
    public async Task<ReportResponse?> GetByIdAsync(long id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            return null;

        return await GetReportResponseAsync(report);
    }

    /// <inheritdoc />
    public async Task<PagedResponse<ReportResponse>> GetAllAsync(ReportQueryParameters parameters)
    {
        var query = _context.Reports.AsQueryable();

        // 필터링
        if (parameters.Status.HasValue)
            query = query.Where(r => r.Status == parameters.Status.Value);

        if (parameters.TargetType.HasValue)
            query = query.Where(r => r.TargetType == parameters.TargetType.Value);

        if (parameters.Reason.HasValue)
            query = query.Where(r => r.Reason == parameters.Reason.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(r => r.CreatedAt <= parameters.ToDate.Value);

        // 정렬
        query = parameters.Sort.ToLower() switch
        {
            "processedat" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(r => r.ProcessedAt) 
                : query.OrderByDescending(r => r.ProcessedAt),
            _ => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(r => r.CreatedAt) 
                : query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var reports = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var responses = new List<ReportResponse>();
        foreach (var report in reports)
        {
            responses.Add(await GetReportResponseAsync(report));
        }

        return PagedResponse<ReportResponse>.Create(
            responses,
            parameters.Page,
            parameters.PageSize,
            totalCount);
    }

    #endregion

    #region 신고 처리

    /// <inheritdoc />
    public async Task<ReportResponse> ProcessAsync(long id, ProcessReportRequest request, long processedById, string processedByName)
    {
        var report = await _context.Reports.FindAsync(id);
        
        if (report == null)
            throw new KeyNotFoundException($"신고를 찾을 수 없습니다: {id}");

        if (report.Status != ReportStatus.Pending)
            throw new InvalidOperationException("이미 처리된 신고입니다.");

        report.Status = request.Status;
        report.ProcessedById = processedById;
        report.ProcessedByName = processedByName;
        report.ProcessedAt = DateTime.UtcNow;
        report.ProcessingNote = request.ProcessingNote;

        // 승인된 경우 대상 콘텐츠 블라인드 처리
        if (request.Status == ReportStatus.Approved)
        {
            await BlindTargetContentAsync(report.TargetType, report.TargetId);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("신고 처리됨: {ReportId}, 상태: {Status}, 처리자: {ProcessedById}", 
            id, request.Status, processedById);

        return await GetReportResponseAsync(report);
    }

    #endregion

    #region 신고 통계

    /// <inheritdoc />
    public async Task<int> GetReportCountAsync(ReportTargetType targetType, long targetId)
    {
        return await _context.Reports
            .CountAsync(r => r.TargetType == targetType && r.TargetId == targetId);
    }

    /// <inheritdoc />
    public async Task<int> GetPendingCountAsync()
    {
        return await _context.Reports
            .CountAsync(r => r.Status == ReportStatus.Pending);
    }

    #endregion

    #region 유틸리티

    /// <inheritdoc />
    public async Task<bool> HasReportedAsync(ReportTargetType targetType, long targetId, long reporterId)
    {
        return await _context.Reports
            .AnyAsync(r => r.TargetType == targetType && 
                          r.TargetId == targetId && 
                          r.ReporterId == reporterId);
    }

    /// <inheritdoc />
    public async Task<bool> TargetExistsAsync(ReportTargetType targetType, long targetId)
    {
        return targetType switch
        {
            ReportTargetType.Post => await _context.Posts.AnyAsync(p => p.Id == targetId),
            ReportTargetType.Comment => await _context.Comments.AnyAsync(c => c.Id == targetId),
            ReportTargetType.Question => await _context.Questions.AnyAsync(q => q.Id == targetId),
            ReportTargetType.Answer => await _context.Answers.AnyAsync(a => a.Id == targetId),
            _ => false
        };
    }

    #endregion

    #region Private Methods

    private async Task<ReportResponse> GetReportResponseAsync(Report report)
    {
        var (targetTitle, targetAuthorName) = await GetTargetInfoAsync(report.TargetType, report.TargetId);

        return new ReportResponse
        {
            Id = report.Id,
            TargetType = report.TargetType,
            TargetTypeText = ReportResponse.GetTargetTypeText(report.TargetType),
            TargetId = report.TargetId,
            TargetTitle = targetTitle,
            TargetAuthorName = targetAuthorName,
            ReporterId = report.ReporterId,
            ReporterName = report.ReporterName,
            Reason = report.Reason,
            ReasonText = ReportResponse.GetReasonText(report.Reason),
            Description = report.Description,
            Status = report.Status,
            StatusText = ReportResponse.GetStatusText(report.Status),
            ProcessedById = report.ProcessedById,
            ProcessedByName = report.ProcessedByName,
            ProcessedAt = report.ProcessedAt,
            ProcessingNote = report.ProcessingNote,
            CreatedAt = report.CreatedAt
        };
    }

    private async Task<(string? title, string? authorName)> GetTargetInfoAsync(ReportTargetType targetType, long targetId)
    {
        return targetType switch
        {
            ReportTargetType.Post => await GetPostInfoAsync(targetId),
            ReportTargetType.Comment => await GetCommentInfoAsync(targetId),
            ReportTargetType.Question => await GetQuestionInfoAsync(targetId),
            ReportTargetType.Answer => await GetAnswerInfoAsync(targetId),
            _ => (null, null)
        };
    }

    private async Task<(string? title, string? authorName)> GetPostInfoAsync(long postId)
    {
        var post = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == postId);
        return (post?.Title, post?.AuthorName);
    }

    private async Task<(string? title, string? authorName)> GetCommentInfoAsync(long commentId)
    {
        var comment = await _context.Comments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        var content = comment?.Content;
        if (content != null && content.Length > 50)
            content = content.Substring(0, 50) + "...";
        return (content, comment?.AuthorName);
    }

    private async Task<(string? title, string? authorName)> GetQuestionInfoAsync(long questionId)
    {
        var question = await _context.Questions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(q => q.Id == questionId);
        return (question?.Title, question?.AuthorName);
    }

    private async Task<(string? title, string? authorName)> GetAnswerInfoAsync(long answerId)
    {
        var answer = await _context.Answers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == answerId);
        var content = answer?.Content;
        if (content != null && content.Length > 50)
            content = content.Substring(0, 50) + "...";
        return (content, answer?.AuthorName);
    }

    private async Task BlindTargetContentAsync(ReportTargetType targetType, long targetId)
    {
        switch (targetType)
        {
            case ReportTargetType.Post:
                var post = await _context.Posts.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == targetId);
                if (post != null)
                {
                    post.IsBlinded = true;
                    post.UpdatedAt = DateTime.UtcNow;
                }
                break;

            case ReportTargetType.Comment:
                var comment = await _context.Comments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == targetId);
                if (comment != null)
                {
                    comment.IsBlinded = true;
                    comment.UpdatedAt = DateTime.UtcNow;
                }
                break;

            case ReportTargetType.Question:
                var question = await _context.Questions.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == targetId);
                if (question != null)
                {
                    question.IsDeleted = true;
                    question.DeletedAt = DateTime.UtcNow;
                }
                break;

            case ReportTargetType.Answer:
                var answer = await _context.Answers.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == targetId);
                if (answer != null)
                {
                    answer.IsDeleted = true;
                    answer.DeletedAt = DateTime.UtcNow;
                }
                break;
        }
    }

    #endregion
}
