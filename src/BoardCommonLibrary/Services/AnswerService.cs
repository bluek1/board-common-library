using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// Q&A 답변 서비스 구현
/// </summary>
public class AnswerService : IAnswerService
{
    private readonly BoardDbContext _context;
    private readonly ILogger<AnswerService> _logger;

    public AnswerService(BoardDbContext context, ILogger<AnswerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region CRUD

    /// <inheritdoc />
    public async Task<AnswerResponse> CreateAsync(long questionId, CreateAnswerRequest request, long authorId, string authorName)
    {
        var question = await _context.Questions.FindAsync(questionId);
        if (question == null)
            throw new KeyNotFoundException($"질문을 찾을 수 없습니다: {questionId}");

        if (question.Status == QuestionStatus.Closed)
            throw new InvalidOperationException("종료된 질문에는 답변할 수 없습니다.");

        var answer = new Answer
        {
            Content = request.Content,
            QuestionId = questionId,
            AuthorId = authorId,
            AuthorName = authorName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Answers.Add(answer);
        
        // 질문의 답변 수 증가
        question.AnswerCount++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 생성됨: {AnswerId}, 질문: {QuestionId}, 작성자: {AuthorId}", 
            answer.Id, questionId, authorId);

        return MapToResponse(answer);
    }

    /// <inheritdoc />
    public async Task<AnswerResponse?> GetByIdAsync(long id, long? currentUserId = null)
    {
        var answer = await _context.Answers
            .Include(a => a.Votes)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (answer == null)
            return null;

        var response = MapToResponse(answer);

        if (currentUserId.HasValue)
        {
            var vote = answer.Votes.FirstOrDefault(v => v.UserId == currentUserId.Value);
            response.CurrentUserVote = vote?.VoteType;
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<List<AnswerResponse>> GetByQuestionIdAsync(long questionId, long? currentUserId = null)
    {
        var answers = await _context.Answers
            .Include(a => a.Votes)
            .Where(a => a.QuestionId == questionId)
            .OrderByDescending(a => a.IsAccepted)
            .ThenByDescending(a => a.VoteCount)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        var responses = answers.Select(MapToResponse).ToList();

        if (currentUserId.HasValue)
        {
            foreach (var response in responses)
            {
                var answer = answers.First(a => a.Id == response.Id);
                var vote = answer.Votes.FirstOrDefault(v => v.UserId == currentUserId.Value);
                response.CurrentUserVote = vote?.VoteType;
            }
        }

        return responses;
    }

    /// <inheritdoc />
    public async Task<AnswerResponse> UpdateAsync(long id, UpdateAnswerRequest request, long userId)
    {
        var answer = await _context.Answers.FindAsync(id);
        
        if (answer == null)
            throw new KeyNotFoundException($"답변을 찾을 수 없습니다: {id}");

        if (answer.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 답변만 수정할 수 있습니다.");

        answer.Content = request.Content;
        answer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 수정됨: {AnswerId}, 수정자: {UserId}", id, userId);

        return MapToResponse(answer);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, long userId)
    {
        var answer = await _context.Answers.FindAsync(id);
        
        if (answer == null)
            throw new KeyNotFoundException($"답변을 찾을 수 없습니다: {id}");

        if (answer.AuthorId != userId)
            throw new UnauthorizedAccessException("본인의 답변만 삭제할 수 있습니다.");

        // 채택된 답변은 삭제 불가
        if (answer.IsAccepted)
            throw new InvalidOperationException("채택된 답변은 삭제할 수 없습니다.");

        // 소프트 삭제
        answer.IsDeleted = true;
        answer.DeletedAt = DateTime.UtcNow;

        // 질문의 답변 수 감소
        var question = await _context.Questions.FindAsync(answer.QuestionId);
        if (question != null)
        {
            question.AnswerCount = Math.Max(0, question.AnswerCount - 1);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 삭제됨: {AnswerId}, 삭제자: {UserId}", id, userId);

        return true;
    }

    #endregion

    #region 채택

    /// <inheritdoc />
    public async Task<AnswerResponse> AcceptAsync(long answerId, long questionAuthorId)
    {
        var answer = await _context.Answers
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == answerId);
        
        if (answer == null)
            throw new KeyNotFoundException($"답변을 찾을 수 없습니다: {answerId}");

        if (answer.Question.AuthorId != questionAuthorId)
            throw new UnauthorizedAccessException("질문 작성자만 답변을 채택할 수 있습니다.");

        if (answer.Question.Status == QuestionStatus.Closed)
            throw new InvalidOperationException("종료된 질문의 답변은 채택할 수 없습니다.");

        // 기존 채택 답변 해제
        if (answer.Question.AcceptedAnswerId.HasValue)
        {
            var previousAccepted = await _context.Answers.FindAsync(answer.Question.AcceptedAnswerId.Value);
            if (previousAccepted != null)
            {
                previousAccepted.IsAccepted = false;
            }
        }

        // 새 답변 채택
        answer.IsAccepted = true;
        answer.Question.AcceptedAnswerId = answerId;
        answer.Question.Status = QuestionStatus.Answered;
        answer.Question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 채택됨: {AnswerId}, 질문: {QuestionId}", answerId, answer.QuestionId);

        return MapToResponse(answer);
    }

    /// <inheritdoc />
    public async Task<bool> UnacceptAsync(long answerId, long questionAuthorId)
    {
        var answer = await _context.Answers
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == answerId);
        
        if (answer == null)
            throw new KeyNotFoundException($"답변을 찾을 수 없습니다: {answerId}");

        if (answer.Question.AuthorId != questionAuthorId)
            throw new UnauthorizedAccessException("질문 작성자만 채택을 취소할 수 있습니다.");

        if (!answer.IsAccepted)
            throw new InvalidOperationException("채택되지 않은 답변입니다.");

        answer.IsAccepted = false;
        answer.Question.AcceptedAnswerId = null;
        answer.Question.Status = QuestionStatus.Open;
        answer.Question.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 채택 취소됨: {AnswerId}, 질문: {QuestionId}", answerId, answer.QuestionId);

        return true;
    }

    #endregion

    #region 추천

    /// <inheritdoc />
    public async Task<AnswerResponse> VoteAsync(long id, long userId, VoteType voteType)
    {
        var answer = await _context.Answers
            .Include(a => a.Votes)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (answer == null)
            throw new KeyNotFoundException($"답변을 찾을 수 없습니다: {id}");

        // 본인 답변에는 투표 불가
        if (answer.AuthorId == userId)
            throw new InvalidOperationException("본인의 답변에는 투표할 수 없습니다.");

        var existingVote = answer.Votes.FirstOrDefault(v => v.UserId == userId);

        if (existingVote != null)
        {
            if (existingVote.VoteType == voteType)
                throw new InvalidOperationException("이미 동일한 투표를 하셨습니다.");

            // 투표 유형 변경
            if (existingVote.VoteType == VoteType.Up)
            {
                answer.UpvoteCount--;
            }
            else
            {
                answer.DownvoteCount--;
            }

            existingVote.VoteType = voteType;

            if (voteType == VoteType.Up)
            {
                answer.UpvoteCount++;
            }
            else
            {
                answer.DownvoteCount++;
            }
        }
        else
        {
            // 새 투표
            var vote = new AnswerVote
            {
                AnswerId = id,
                UserId = userId,
                VoteType = voteType,
                CreatedAt = DateTime.UtcNow
            };
            _context.AnswerVotes.Add(vote);

            if (voteType == VoteType.Up)
            {
                answer.UpvoteCount++;
            }
            else
            {
                answer.DownvoteCount++;
            }
        }

        // VoteCount 갱신
        answer.VoteCount = answer.UpvoteCount - answer.DownvoteCount;

        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 투표: {AnswerId}, 사용자: {UserId}, 유형: {VoteType}", id, userId, voteType);

        return MapToResponse(answer);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveVoteAsync(long id, long userId)
    {
        var vote = await _context.AnswerVotes
            .FirstOrDefaultAsync(v => v.AnswerId == id && v.UserId == userId);

        if (vote == null)
            return false;

        var answer = await _context.Answers.FindAsync(id);
        if (answer != null)
        {
            if (vote.VoteType == VoteType.Up)
            {
                answer.UpvoteCount--;
            }
            else
            {
                answer.DownvoteCount--;
            }
            answer.VoteCount = answer.UpvoteCount - answer.DownvoteCount;
        }

        _context.AnswerVotes.Remove(vote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("답변 투표 취소: {AnswerId}, 사용자: {UserId}", id, userId);

        return true;
    }

    #endregion

    #region 유틸리티

    /// <inheritdoc />
    public async Task<bool> IsAuthorAsync(long answerId, long userId)
    {
        var answer = await _context.Answers.FindAsync(answerId);
        return answer?.AuthorId == userId;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Answers.AnyAsync(a => a.Id == id);
    }

    #endregion

    #region Private Methods

    private static AnswerResponse MapToResponse(Answer answer)
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
