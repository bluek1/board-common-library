using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// Q&A 질문 서비스 구현
/// </summary>
public class QuestionService : IQuestionService
{
    private readonly BoardDbContext _context;
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(BoardDbContext context, ILogger<QuestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region CRUD

    /// <inheritdoc />
    public async Task<QuestionResponse> CreateAsync(CreateQuestionRequest request, long authorId, string authorName)
    {
        var question = new Question
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = authorId,
            AuthorName = authorName,
            Tags = request.Tags ?? new List<string>(),
            BountyPoints = request.BountyPoints,
            Status = QuestionStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 생성됨: {QuestionId}, 작성자: {AuthorId}", question.Id, authorId);

        return MapToResponse(question);
    }

    /// <inheritdoc />
    public async Task<QuestionDetailResponse?> GetByIdAsync(long id, long? currentUserId = null)
    {
        var question = await _context.Questions
            .Include(q => q.Answers.Where(a => !a.IsDeleted))
            .Include(q => q.Votes)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return null;

        var response = MapToDetailResponse(question);

        // 현재 사용자의 투표 상태 확인
        if (currentUserId.HasValue)
        {
            var vote = question.Votes.FirstOrDefault(v => v.UserId == currentUserId.Value);
            response.CurrentUserVote = vote?.VoteType;

            // 답변에 대한 투표 상태도 확인
            foreach (var answerResponse in response.Answers)
            {
                var answer = question.Answers.FirstOrDefault(a => a.Id == answerResponse.Id);
                if (answer != null)
                {
                    await _context.Entry(answer).Collection(a => a.Votes).LoadAsync();
                    var answerVote = answer.Votes.FirstOrDefault(v => v.UserId == currentUserId.Value);
                    answerResponse.CurrentUserVote = answerVote?.VoteType;
                }
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<PagedResponse<QuestionResponse>> GetAllAsync(QuestionQueryParameters parameters)
    {
        var query = _context.Questions
            .Include(q => q.Votes)
            .AsQueryable();

        // 필터링
        if (parameters.Status.HasValue)
            query = query.Where(q => q.Status == parameters.Status.Value);

        if (parameters.AuthorId.HasValue)
            query = query.Where(q => q.AuthorId == parameters.AuthorId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Tag))
            query = query.Where(q => q.Tags.Contains(parameters.Tag));

        if (!string.IsNullOrWhiteSpace(parameters.Query))
        {
            var searchTerm = parameters.Query.ToLower();
            query = query.Where(q => 
                q.Title.ToLower().Contains(searchTerm) || 
                q.Content.ToLower().Contains(searchTerm));
        }

        // 정렬
        query = parameters.Sort.ToLower() switch
        {
            "viewcount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(q => q.ViewCount) 
                : query.OrderByDescending(q => q.ViewCount),
            "votecount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(q => q.VoteCount) 
                : query.OrderByDescending(q => q.VoteCount),
            "answercount" => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(q => q.AnswerCount) 
                : query.OrderByDescending(q => q.AnswerCount),
            _ => parameters.Order.ToLower() == "asc" 
                ? query.OrderBy(q => q.CreatedAt) 
                : query.OrderByDescending(q => q.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var questions = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var responses = questions.Select(MapToResponse).ToList();

        return PagedResponse<QuestionResponse>.Create(
            responses,
            parameters.Page,
            parameters.PageSize,
            totalCount);
    }

    /// <inheritdoc />
    public async Task<QuestionResponse> UpdateAsync(long id, UpdateQuestionRequest request, long userId)
    {
        var question = await _context.Questions.FindAsync(id);
        
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {id}");

        if (question.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 질문만 수정할 수 있습니다.");

        question.Title = request.Title;
        question.Content = request.Content;
        question.Tags = request.Tags ?? new List<string>();
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 수정됨: {QuestionId}, 수정자: {UserId}", id, userId);

        return MapToResponse(question);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, long userId)
    {
        var question = await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
        
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {id}");

        if (question.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 질문만 삭제할 수 있습니다.");

        // 답변이 있는 경우 삭제 불가
        if (question.Answers.Any(a => !a.IsDeleted))
            throw new InvalidOperationException("답변이 있는 질문은 삭제할 수 없습니다.");

        // 소프트 삭제
        question.IsDeleted = true;
        question.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 삭제됨: {QuestionId}, 삭제자: {UserId}", id, userId);

        return true;
    }

    #endregion

    #region 조회수

    /// <inheritdoc />
    public async Task IncrementViewCountAsync(long id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            question.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region 상태 관리

    /// <inheritdoc />
    public async Task<QuestionResponse> CloseAsync(long id, long userId)
    {
        var question = await _context.Questions.FindAsync(id);
        
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {id}");

        if (question.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 질문만 종료할 수 있습니다.");

        question.Status = QuestionStatus.Closed;
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 종료됨: {QuestionId}, 종료자: {UserId}", id, userId);

        return MapToResponse(question);
    }

    /// <inheritdoc />
    public async Task<QuestionResponse> ReopenAsync(long id, long userId)
    {
        var question = await _context.Questions.FindAsync(id);
        
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {id}");

        if (question.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 질문만 다시 열 수 있습니다.");

        question.Status = QuestionStatus.Open;
        question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 다시 열림: {QuestionId}, 요청자: {UserId}", id, userId);

        return MapToResponse(question);
    }

    #endregion

    #region 추천

    /// <inheritdoc />
    public async Task<int> VoteAsync(long id, long userId, VoteType voteType)
    {
        var question = await _context.Questions
            .Include(q => q.Votes)
            .FirstOrDefaultAsync(q => q.Id == id);
        
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {id}");

        // 본인 질문에는 투표 불가
        if (question.AuthorId == userId)
            throw new InvalidOperationException("본인의 질문에는 투표할 수 없습니다.");

        var existingVote = question.Votes.FirstOrDefault(v => v.UserId == userId);

        if (existingVote != null)
        {
            if (existingVote.VoteType == voteType)
                throw new InvalidOperationException("이미 동일한 투표를 하셨습니다.");

            // 투표 유형 변경
            var oldVoteValue = (int)existingVote.VoteType;
            var newVoteValue = (int)voteType;
            question.VoteCount = question.VoteCount - oldVoteValue + newVoteValue;
            existingVote.VoteType = voteType;
        }
        else
        {
            // 새 투표
            var vote = new QuestionVote
            {
                QuestionId = id,
                UserId = userId,
                VoteType = voteType,
                CreatedAt = DateTime.UtcNow
            };
            _context.QuestionVotes.Add(vote);
            question.VoteCount += (int)voteType;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 투표: {QuestionId}, 사용자: {UserId}, 유형: {VoteType}", id, userId, voteType);

        return question.VoteCount;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveVoteAsync(long id, long userId)
    {
        var vote = await _context.QuestionVotes
            .FirstOrDefaultAsync(v => v.QuestionId == id && v.UserId == userId);

        if (vote == null)
            return false;

        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            question.VoteCount -= (int)vote.VoteType;
        }

        _context.QuestionVotes.Remove(vote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("질문 투표 취소: {QuestionId}, 사용자: {UserId}", id, userId);

        return true;
    }

    #endregion

    #region 유틸리티

    /// <inheritdoc />
    public async Task<bool> IsAuthorAsync(long questionId, long userId)
    {
        var question = await _context.Questions.FindAsync(questionId);
        return question?.AuthorId == userId;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Questions.AnyAsync(q => q.Id == id);
    }

    #endregion

    #region Private Methods

    private static QuestionResponse MapToResponse(Question question)
    {
        return new QuestionResponse
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            AuthorId = question.AuthorId,
            AuthorName = question.AuthorName,
            Status = question.Status,
            StatusText = QuestionResponse.GetStatusText(question.Status),
            ViewCount = question.ViewCount,
            VoteCount = question.VoteCount,
            AnswerCount = question.AnswerCount,
            BountyPoints = question.BountyPoints,
            Tags = question.Tags,
            AcceptedAnswerId = question.AcceptedAnswerId,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt
        };
    }

    private static QuestionDetailResponse MapToDetailResponse(Question question)
    {
        var response = new QuestionDetailResponse
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            AuthorId = question.AuthorId,
            AuthorName = question.AuthorName,
            Status = question.Status,
            StatusText = QuestionResponse.GetStatusText(question.Status),
            ViewCount = question.ViewCount,
            VoteCount = question.VoteCount,
            AnswerCount = question.AnswerCount,
            BountyPoints = question.BountyPoints,
            Tags = question.Tags,
            AcceptedAnswerId = question.AcceptedAnswerId,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Answers = question.Answers
                .OrderByDescending(a => a.IsAccepted)
                .ThenByDescending(a => a.VoteCount)
                .ThenByDescending(a => a.CreatedAt)
                .Select(MapToAnswerResponse)
                .ToList()
        };

        if (question.AcceptedAnswerId.HasValue)
        {
            response.AcceptedAnswer = response.Answers
                .FirstOrDefault(a => a.Id == question.AcceptedAnswerId.Value);
        }

        return response;
    }

    private static AnswerResponse MapToAnswerResponse(Answer answer)
    {
        return new AnswerResponse
        {
            Id = answer.Id,
            Content = answer.Content,
            QuestionId = answer.QuestionId,
            AuthorId = answer.AuthorId,
            AuthorName = answer.AuthorName,
            IsAccepted = answer.IsAccepted,
            VoteCount = answer.VoteCount,
            UpvoteCount = answer.UpvoteCount,
            DownvoteCount = answer.DownvoteCount,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt
        };
    }

    #endregion
}
