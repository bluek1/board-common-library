using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 관리자 서비스 구현
/// </summary>
public class AdminService : IAdminService
{
    private readonly BoardDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(BoardDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region 게시물 관리

    /// <inheritdoc />
    public async Task<PagedResponse<PostResponse>> GetAllPostsAsync(AdminPostQueryParameters parameters)
    {
        var query = _context.Posts
            .IgnoreQueryFilters() // 삭제된 게시물도 포함
            .AsQueryable();

        // 필터링
        if (parameters.Status.HasValue)
            query = query.Where(p => p.Status == parameters.Status.Value);

        if (parameters.IsDeleted.HasValue)
            query = query.Where(p => p.IsDeleted == parameters.IsDeleted.Value);

        if (parameters.IsBlinded.HasValue)
            query = query.Where(p => p.IsBlinded == parameters.IsBlinded.Value);

        if (parameters.AuthorId.HasValue)
            query = query.Where(p => p.AuthorId == parameters.AuthorId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Category))
            query = query.Where(p => p.Category == parameters.Category);

        if (parameters.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= parameters.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Query))
        {
            var searchTerm = parameters.Query.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchTerm) || 
                p.Content.ToLower().Contains(searchTerm));
        }

        // 정렬
        query = parameters.Sort.ToLower() switch
        {
            "viewcount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(p => p.ViewCount) 
                : query.OrderByDescending(p => p.ViewCount),
            "likecount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(p => p.LikeCount) 
                : query.OrderByDescending(p => p.LikeCount),
            _ => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(p => p.CreatedAt) 
                : query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var responses = posts.Select(MapToPostResponse).ToList();

        return PagedResponse<PostResponse>.Create(
            responses,
            parameters.Page,
            parameters.PageSize,
            totalCount);
    }

    #endregion

    #region 댓글 관리

    /// <inheritdoc />
    public async Task<PagedResponse<CommentResponse>> GetAllCommentsAsync(AdminCommentQueryParameters parameters)
    {
        var query = _context.Comments
            .IgnoreQueryFilters() // 삭제된 댓글도 포함
            .AsQueryable();

        // 필터링
        if (parameters.IsDeleted.HasValue)
            query = query.Where(c => c.IsDeleted == parameters.IsDeleted.Value);

        if (parameters.IsBlinded.HasValue)
            query = query.Where(c => c.IsBlinded == parameters.IsBlinded.Value);

        if (parameters.AuthorId.HasValue)
            query = query.Where(c => c.AuthorId == parameters.AuthorId.Value);

        if (parameters.PostId.HasValue)
            query = query.Where(c => c.PostId == parameters.PostId.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(c => c.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(c => c.CreatedAt <= parameters.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Query))
        {
            var searchTerm = parameters.Query.ToLower();
            query = query.Where(c => c.Content.ToLower().Contains(searchTerm));
        }

        // 정렬
        query = parameters.Sort.ToLower() switch
        {
            "likecount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(c => c.LikeCount) 
                : query.OrderByDescending(c => c.LikeCount),
            _ => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(c => c.CreatedAt) 
                : query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var comments = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var responses = comments.Select(MapToCommentResponse).ToList();

        return PagedResponse<CommentResponse>.Create(
            responses,
            parameters.Page,
            parameters.PageSize,
            totalCount);
    }

    #endregion

    #region 일괄 처리

    /// <inheritdoc />
    public async Task<BatchDeleteResponse> BatchDeleteAsync(BatchDeleteRequest request)
    {
        var response = new BatchDeleteResponse();

        foreach (var id in request.Ids)
        {
            try
            {
                var deleted = await DeleteContentAsync(request.TargetType, id, request.HardDelete);
                if (deleted)
                    response.SuccessCount++;
                else
                {
                    response.FailedCount++;
                    response.FailedIds.Add(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "일괄 삭제 실패: {TargetType} {Id}", request.TargetType, id);
                response.FailedCount++;
                response.FailedIds.Add(id);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("일괄 삭제 완료: {TargetType}, 성공: {SuccessCount}, 실패: {FailedCount}", 
            request.TargetType, response.SuccessCount, response.FailedCount);

        return response;
    }

    /// <inheritdoc />
    public async Task<bool> BlindContentAsync(BlindContentRequest request)
    {
        try
        {
            switch (request.TargetType)
            {
                case BatchTargetType.Post:
                    var post = await _context.Posts.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == request.TargetId);
                    if (post == null) return false;
                    post.IsBlinded = request.IsBlinded;
                    post.UpdatedAt = DateTime.UtcNow;
                    break;

                case BatchTargetType.Comment:
                    var comment = await _context.Comments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == request.TargetId);
                    if (comment == null) return false;
                    comment.IsBlinded = request.IsBlinded;
                    comment.UpdatedAt = DateTime.UtcNow;
                    break;

                case BatchTargetType.Question:
                    var question = await _context.Questions.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == request.TargetId);
                    if (question == null) return false;
                    // Question은 IsBlinded 대신 IsDeleted 사용
                    question.IsDeleted = request.IsBlinded;
                    question.DeletedAt = request.IsBlinded ? DateTime.UtcNow : null;
                    break;

                case BatchTargetType.Answer:
                    var answer = await _context.Answers.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == request.TargetId);
                    if (answer == null) return false;
                    // Answer는 IsBlinded 대신 IsDeleted 사용
                    answer.IsDeleted = request.IsBlinded;
                    answer.DeletedAt = request.IsBlinded ? DateTime.UtcNow : null;
                    break;

                default:
                    return false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("콘텐츠 블라인드 처리: {TargetType} {TargetId}, IsBlinded: {IsBlinded}", 
                request.TargetType, request.TargetId, request.IsBlinded);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "블라인드 처리 실패: {TargetType} {TargetId}", request.TargetType, request.TargetId);
            return false;
        }
    }

    #endregion

    #region 통계

    /// <inheritdoc />
    public async Task<BoardStatisticsResponse> GetStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var response = new BoardStatisticsResponse
        {
            // 기본 통계
            TotalPosts = await _context.Posts.CountAsync(),
            TotalComments = await _context.Comments.CountAsync(),
            TotalQuestions = await _context.Questions.CountAsync(),
            TotalAnswers = await _context.Answers.CountAsync(),
            TotalFiles = await _context.FileAttachments.CountAsync(),

            // 오늘 통계
            TodayPosts = await _context.Posts.CountAsync(p => p.CreatedAt >= todayStart),
            TodayComments = await _context.Comments.CountAsync(c => c.CreatedAt >= todayStart),
            TodayQuestions = await _context.Questions.CountAsync(q => q.CreatedAt >= todayStart),
            TodayAnswers = await _context.Answers.CountAsync(a => a.CreatedAt >= todayStart),

            // 이번 주 통계
            WeeklyPosts = await _context.Posts.CountAsync(p => p.CreatedAt >= weekStart),
            WeeklyComments = await _context.Comments.CountAsync(c => c.CreatedAt >= weekStart),
            WeeklyQuestions = await _context.Questions.CountAsync(q => q.CreatedAt >= weekStart),
            WeeklyAnswers = await _context.Answers.CountAsync(a => a.CreatedAt >= weekStart),

            // 이번 달 통계
            MonthlyPosts = await _context.Posts.CountAsync(p => p.CreatedAt >= monthStart),
            MonthlyComments = await _context.Comments.CountAsync(c => c.CreatedAt >= monthStart),
            MonthlyQuestions = await _context.Questions.CountAsync(q => q.CreatedAt >= monthStart),
            MonthlyAnswers = await _context.Answers.CountAsync(a => a.CreatedAt >= monthStart),

            // 활동 통계
            TotalViews = await _context.Posts.SumAsync(p => (long)p.ViewCount),
            TotalLikes = await _context.Likes.CountAsync(),

            // 신고 통계
            PendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending),
            TotalReports = await _context.Reports.CountAsync()
        };

        // 인기 게시물 (조회수 기준 상위 5개)
        response.PopularPosts = await _context.Posts
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .Select(p => new PopularPostResponse
            {
                Id = p.Id,
                Title = p.Title,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount
            })
            .ToListAsync();

        // 인기 질문 (조회수 기준 상위 5개)
        response.PopularQuestions = await _context.Questions
            .OrderByDescending(q => q.ViewCount)
            .Take(5)
            .Select(q => new PopularQuestionResponse
            {
                Id = q.Id,
                Title = q.Title,
                ViewCount = q.ViewCount,
                VoteCount = q.VoteCount,
                AnswerCount = q.AnswerCount
            })
            .ToListAsync();

        // 일별 트렌드 (최근 7일)
        for (var i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var nextDate = date.AddDays(1);

            response.DailyTrend.Add(new DailyStatistics
            {
                Date = date,
                PostCount = await _context.Posts.CountAsync(p => p.CreatedAt >= date && p.CreatedAt < nextDate),
                CommentCount = await _context.Comments.CountAsync(c => c.CreatedAt >= date && c.CreatedAt < nextDate),
                QuestionCount = await _context.Questions.CountAsync(q => q.CreatedAt >= date && q.CreatedAt < nextDate),
                AnswerCount = await _context.Answers.CountAsync(a => a.CreatedAt >= date && a.CreatedAt < nextDate)
            });
        }

        return response;
    }

    #endregion

    #region Private Methods

    private async Task<bool> DeleteContentAsync(BatchTargetType targetType, long id, bool hardDelete)
    {
        switch (targetType)
        {
            case BatchTargetType.Post:
                var post = await _context.Posts.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
                if (post == null) return false;
                if (hardDelete)
                    _context.Posts.Remove(post);
                else
                {
                    post.IsDeleted = true;
                    post.DeletedAt = DateTime.UtcNow;
                }
                return true;

            case BatchTargetType.Comment:
                var comment = await _context.Comments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
                if (comment == null) return false;
                if (hardDelete)
                    _context.Comments.Remove(comment);
                else
                {
                    comment.IsDeleted = true;
                    comment.DeletedAt = DateTime.UtcNow;
                }
                return true;

            case BatchTargetType.Question:
                var question = await _context.Questions.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == id);
                if (question == null) return false;
                if (hardDelete)
                    _context.Questions.Remove(question);
                else
                {
                    question.IsDeleted = true;
                    question.DeletedAt = DateTime.UtcNow;
                }
                return true;

            case BatchTargetType.Answer:
                var answer = await _context.Answers.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
                if (answer == null) return false;
                if (hardDelete)
                    _context.Answers.Remove(answer);
                else
                {
                    answer.IsDeleted = true;
                    answer.DeletedAt = DateTime.UtcNow;
                }
                return true;

            default:
                return false;
        }
    }

    private static PostResponse MapToPostResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            AuthorId = post.AuthorId,
            AuthorName = post.AuthorName ?? string.Empty,
            Category = post.Category,
            Tags = post.Tags,
            Status = post.Status,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsPinned = post.IsPinned,
            IsDeleted = post.IsDeleted,
            IsBlinded = post.IsBlinded,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static CommentResponse MapToCommentResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.AuthorName ?? string.Empty,
            ParentId = comment.ParentId,
            LikeCount = comment.LikeCount,
            IsDeleted = comment.IsDeleted,
            IsBlinded = comment.IsBlinded,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    #endregion
}
