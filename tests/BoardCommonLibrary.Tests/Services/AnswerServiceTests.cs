using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// AnswerService 단위 테스트
/// </summary>
public class AnswerServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly AnswerService _service;
    private readonly Mock<ILogger<AnswerService>> _loggerMock;
    private readonly Question _testQuestion;

    public AnswerServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _loggerMock = new Mock<ILogger<AnswerService>>();
        _service = new AnswerService(_context, _loggerMock.Object);

        // 테스트용 질문 생성
        _testQuestion = new Question
        {
            Title = "테스트 질문",
            Content = "테스트 질문 내용",
            AuthorId = 1,
            AuthorName = "질문자",
            Status = QuestionStatus.Open
        };
        _context.Questions.Add(_testQuestion);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateAnswer()
    {
        // Arrange
        var request = new CreateAnswerRequest
        {
            Content = "테스트 답변입니다."
        };

        // Act
        var result = await _service.CreateAsync(_testQuestion.Id, request, authorId: 2, authorName: "답변자");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Content.Should().Be("테스트 답변입니다.");
        result.QuestionId.Should().Be(_testQuestion.Id);
        result.AuthorId.Should().Be(2);
        result.AuthorName.Should().Be("답변자");
        result.IsAccepted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldIncrementQuestionAnswerCount()
    {
        // Arrange
        var request = new CreateAnswerRequest { Content = "답변" };
        var initialCount = _testQuestion.AnswerCount;

        // Act
        await _service.CreateAsync(_testQuestion.Id, request, authorId: 2, authorName: "답변자");

        // Assert
        await _context.Entry(_testQuestion).ReloadAsync();
        _testQuestion.AnswerCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingQuestion_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new CreateAnswerRequest { Content = "답변" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync(999, request, authorId: 2, authorName: "답변자"));
    }

    [Fact]
    public async Task CreateAsync_OnClosedQuestion_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _testQuestion.Status = QuestionStatus.Closed;
        await _context.SaveChangesAsync();

        var request = new CreateAnswerRequest { Content = "답변" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(_testQuestion.Id, request, authorId: 2, authorName: "답변자"));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnAnswer()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "테스트 답변",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(answer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(answer.Id);
        result.Content.Should().Be("테스트 답변");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByQuestionIdAsync Tests

    [Fact]
    public async Task GetByQuestionIdAsync_WithAnswers_ShouldReturnAllAnswers()
    {
        // Arrange
        _context.Answers.AddRange(
            new Answer { QuestionId = _testQuestion.Id, Content = "답변 1", AuthorId = 2, AuthorName = "답변자1" },
            new Answer { QuestionId = _testQuestion.Id, Content = "답변 2", AuthorId = 3, AuthorName = "답변자2" },
            new Answer { QuestionId = _testQuestion.Id, Content = "답변 3", AuthorId = 4, AuthorName = "답변자3" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByQuestionIdAsync(_testQuestion.Id);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByQuestionIdAsync_ShouldOrderByAcceptedFirst()
    {
        // Arrange
        _context.Answers.AddRange(
            new Answer { QuestionId = _testQuestion.Id, Content = "일반 답변", AuthorId = 2, AuthorName = "답변자1", IsAccepted = false },
            new Answer { QuestionId = _testQuestion.Id, Content = "채택된 답변", AuthorId = 3, AuthorName = "답변자2", IsAccepted = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByQuestionIdAsync(_testQuestion.Id);

        // Assert
        result.First().IsAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByQuestionIdAsync_WithNoAnswers_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetByQuestionIdAsync(_testQuestion.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithAuthor_ShouldUpdateAnswer()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "원래 답변",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateAnswerRequest { Content = "수정된 답변" };

        // Act
        var result = await _service.UpdateAsync(answer.Id, updateRequest, userId: 2);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("수정된 답변");
    }

    [Fact]
    public async Task UpdateAsync_WithNonAuthor_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "원래 답변",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateAnswerRequest { Content = "수정 시도" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(answer.Id, updateRequest, userId: 3));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingAnswer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var updateRequest = new UpdateAnswerRequest { Content = "수정" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, updateRequest, userId: 1));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithAuthor_ShouldDeleteAnswer()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "삭제 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(answer.Id, userId: 2);

        // Assert
        result.Should().BeTrue();
        var deletedAnswer = await _context.Answers.FindAsync(answer.Id);
        deletedAnswer!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDecrementQuestionAnswerCount()
    {
        // Arrange
        _testQuestion.AnswerCount = 1;
        await _context.SaveChangesAsync();

        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "삭제 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(answer.Id, userId: 2);

        // Assert
        await _context.Entry(_testQuestion).ReloadAsync();
        _testQuestion.AnswerCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_AcceptedAnswer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택된 답변",
            AuthorId = 2,
            AuthorName = "답변자",
            IsAccepted = true
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(answer.Id, userId: 2));
    }

    #endregion

    #region AcceptAsync Tests

    [Fact]
    public async Task AcceptAsync_WithQuestionAuthor_ShouldAcceptAnswer()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            IsAccepted = false
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptAsync(answer.Id, questionAuthorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.IsAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task AcceptAsync_ShouldUpdateQuestionStatus()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        await _service.AcceptAsync(answer.Id, questionAuthorId: 1);

        // Assert
        await _context.Entry(_testQuestion).ReloadAsync();
        _testQuestion.Status.Should().Be(QuestionStatus.Answered);
        _testQuestion.AcceptedAnswerId.Should().Be(answer.Id);
    }

    [Fact]
    public async Task AcceptAsync_WithNonQuestionAuthor_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AcceptAsync(answer.Id, questionAuthorId: 3));
    }

    [Fact]
    public async Task AcceptAsync_ShouldUnacceptPreviousAnswer()
    {
        // Arrange
        var previousAnswer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "이전 채택 답변",
            AuthorId = 2,
            AuthorName = "답변자1",
            IsAccepted = true
        };
        var newAnswer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "새 답변",
            AuthorId = 3,
            AuthorName = "답변자2"
        };
        _context.Answers.AddRange(previousAnswer, newAnswer);
        _testQuestion.AcceptedAnswerId = previousAnswer.Id;
        await _context.SaveChangesAsync();

        // Act
        await _service.AcceptAsync(newAnswer.Id, questionAuthorId: 1);

        // Assert
        await _context.Entry(previousAnswer).ReloadAsync();
        previousAnswer.IsAccepted.Should().BeFalse();
    }

    #endregion

    #region UnacceptAsync Tests

    [Fact]
    public async Task UnacceptAsync_WithAcceptedAnswer_ShouldUnaccept()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택 취소 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            IsAccepted = true
        };
        _context.Answers.Add(answer);
        _testQuestion.AcceptedAnswerId = answer.Id;
        _testQuestion.Status = QuestionStatus.Answered;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UnacceptAsync(answer.Id, questionAuthorId: 1);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(answer).ReloadAsync();
        answer.IsAccepted.Should().BeFalse();
    }

    [Fact]
    public async Task UnacceptAsync_ShouldUpdateQuestionStatus()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "채택 취소 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            IsAccepted = true
        };
        _context.Answers.Add(answer);
        _testQuestion.AcceptedAnswerId = answer.Id;
        _testQuestion.Status = QuestionStatus.Answered;
        await _context.SaveChangesAsync();

        // Act
        await _service.UnacceptAsync(answer.Id, questionAuthorId: 1);

        // Assert
        await _context.Entry(_testQuestion).ReloadAsync();
        _testQuestion.Status.Should().Be(QuestionStatus.Open);
        _testQuestion.AcceptedAnswerId.Should().BeNull();
    }

    [Fact]
    public async Task UnacceptAsync_NotAcceptedAnswer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "미채택 답변",
            AuthorId = 2,
            AuthorName = "답변자",
            IsAccepted = false
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UnacceptAsync(answer.Id, questionAuthorId: 1));
    }

    #endregion

    #region VoteAsync Tests

    [Fact]
    public async Task VoteAsync_WithUpvote_ShouldIncreaseVoteCount()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "투표 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            VoteCount = 0,
            UpvoteCount = 0
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoteAsync(answer.Id, userId: 3, VoteType.Up);

        // Assert
        result.Should().NotBeNull();
        result.VoteCount.Should().Be(1);
        result.UpvoteCount.Should().Be(1);
    }

    [Fact]
    public async Task VoteAsync_WithDownvote_ShouldDecreaseVoteCount()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "투표 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            VoteCount = 5,
            UpvoteCount = 5,
            DownvoteCount = 0
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoteAsync(answer.Id, userId: 3, VoteType.Down);

        // Assert
        result.Should().NotBeNull();
        result.VoteCount.Should().Be(4);
        result.DownvoteCount.Should().Be(1);
    }

    [Fact]
    public async Task VoteAsync_OnOwnAnswer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "투표 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VoteAsync(answer.Id, userId: 2, VoteType.Up));
    }

    #endregion

    #region RemoveVoteAsync Tests

    [Fact]
    public async Task RemoveVoteAsync_WithExistingUpvote_ShouldRemoveVote()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "투표 제거 테스트",
            AuthorId = 2,
            AuthorName = "답변자",
            VoteCount = 1,
            UpvoteCount = 1
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        _context.AnswerVotes.Add(new AnswerVote
        {
            AnswerId = answer.Id,
            UserId = 3,
            VoteType = VoteType.Up
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVoteAsync(answer.Id, userId: 3);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(answer).ReloadAsync();
        answer.VoteCount.Should().Be(0);
        answer.UpvoteCount.Should().Be(0);
    }

    [Fact]
    public async Task RemoveVoteAsync_WithNoExistingVote_ShouldReturnFalse()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "투표 제거 테스트",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVoteAsync(answer.Id, userId: 3);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public async Task ExistsAsync_WithExistingAnswer_ShouldReturnTrue()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "존재 확인",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ExistsAsync(answer.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingAnswer_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthorAsync_WithAuthor_ShouldReturnTrue()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "작성자 확인",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsAuthorAsync(answer.Id, userId: 2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthorAsync_WithNonAuthor_ShouldReturnFalse()
    {
        // Arrange
        var answer = new Answer
        {
            QuestionId = _testQuestion.Id,
            Content = "작성자 확인",
            AuthorId = 2,
            AuthorName = "답변자"
        };
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsAuthorAsync(answer.Id, userId: 3);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
