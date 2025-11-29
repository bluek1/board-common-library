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
/// QuestionService 단위 테스트
/// </summary>
public class QuestionServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly QuestionService _service;
    private readonly Mock<ILogger<QuestionService>> _loggerMock;

    public QuestionServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _loggerMock = new Mock<ILogger<QuestionService>>();
        _service = new QuestionService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateQuestion()
    {
        // Arrange
        var request = new CreateQuestionRequest
        {
            Title = "테스트 질문",
            Content = "테스트 질문 내용입니다.",
            Tags = new List<string> { "C#", "테스트" }
        };

        // Act
        var result = await _service.CreateAsync(request, authorId: 1, authorName: "테스터");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("테스트 질문");
        result.Content.Should().Be("테스트 질문 내용입니다.");
        result.AuthorId.Should().Be(1);
        result.AuthorName.Should().Be("테스터");
        result.Status.Should().Be(QuestionStatus.Open);
    }

    [Fact]
    public async Task CreateAsync_WithBountyPoints_ShouldSetBounty()
    {
        // Arrange
        var request = new CreateQuestionRequest
        {
            Title = "현상금 질문",
            Content = "현상금이 있는 질문",
            BountyPoints = 100
        };

        // Act
        var result = await _service.CreateAsync(request, authorId: 1, authorName: "테스터");

        // Assert
        result.BountyPoints.Should().Be(100);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingQuestion_ShouldReturnDetailResponse()
    {
        // Arrange
        var question = new Question
        {
            Title = "조회 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            Status = QuestionStatus.Open
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(question.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(question.Id);
        result.Title.Should().Be("조회 테스트");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithAnswers_ShouldIncludeAnswers()
    {
        // Arrange
        var question = new Question
        {
            Title = "답변이 있는 질문",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            Status = QuestionStatus.Open
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        _context.Answers.Add(new Answer
        {
            QuestionId = question.Id,
            Content = "답변입니다",
            AuthorId = 2,
            AuthorName = "답변자"
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(question.Id);

        // Assert
        result!.Answers.Should().HaveCount(1);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithQuestions_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.Questions.Add(new Question
            {
                Title = $"질문 {i + 1}",
                Content = $"내용 {i + 1}",
                AuthorId = 1,
                AuthorName = "작성자"
            });
        }
        await _context.SaveChangesAsync();

        var parameters = new QuestionQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Meta.TotalCount.Should().Be(15);
        result.Meta.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Questions.AddRange(
            new Question { Title = "열린 질문", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = QuestionStatus.Open },
            new Question { Title = "닫힌 질문", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = QuestionStatus.Closed },
            new Question { Title = "열린 질문 2", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = QuestionStatus.Open }
        );
        await _context.SaveChangesAsync();

        var parameters = new QuestionQueryParameters { Status = QuestionStatus.Open };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(q => q.Status == QuestionStatus.Open);
    }

    [Fact]
    public async Task GetAllAsync_FilterByQuery_ShouldReturnMatchingResults()
    {
        // Arrange
        _context.Questions.AddRange(
            new Question { Title = "C# 질문", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Question { Title = "Java 질문", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Question { Title = "Python", Content = "C# 관련 내용", AuthorId = 1, AuthorName = "작성자" }
        );
        await _context.SaveChangesAsync();

        var parameters = new QuestionQueryParameters { Query = "C#" };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_FilterByTag_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Questions.AddRange(
            new Question { Title = "질문 1", Content = "내용", AuthorId = 1, AuthorName = "작성자", Tags = new List<string> { "C#", "Entity Framework" } },
            new Question { Title = "질문 2", Content = "내용", AuthorId = 1, AuthorName = "작성자", Tags = new List<string> { "Java" } }
        );
        await _context.SaveChangesAsync();

        var parameters = new QuestionQueryParameters { Tag = "C#" };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithAuthor_ShouldUpdateQuestion()
    {
        // Arrange
        var question = new Question
        {
            Title = "원래 제목",
            Content = "원래 내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var request = new UpdateQuestionRequest
        {
            Title = "수정된 제목",
            Content = "수정된 내용"
        };

        // Act
        var result = await _service.UpdateAsync(question.Id, request, userId: 1);

        // Assert
        result.Title.Should().Be("수정된 제목");
        result.Content.Should().Be("수정된 내용");
    }

    [Fact]
    public async Task UpdateAsync_WithNonAuthor_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var question = new Question
        {
            Title = "제목",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var request = new UpdateQuestionRequest { Title = "수정", Content = "수정" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(question.Id, request, userId: 2));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingQuestion_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateQuestionRequest { Title = "수정", Content = "수정" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, request, userId: 1));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithAuthorAndNoAnswers_ShouldDeleteQuestion()
    {
        // Arrange
        var question = new Question
        {
            Title = "삭제 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(question.Id, userId: 1);

        // Assert
        result.Should().BeTrue();
        var deletedQuestion = await _context.Questions.FindAsync(question.Id);
        deletedQuestion!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithAnswers_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var question = new Question
        {
            Title = "삭제 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        _context.Answers.Add(new Answer
        {
            QuestionId = question.Id,
            Content = "답변",
            AuthorId = 2,
            AuthorName = "답변자"
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(question.Id, userId: 1));
    }

    [Fact]
    public async Task DeleteAsync_WithNonAuthor_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var question = new Question
        {
            Title = "삭제 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteAsync(question.Id, userId: 2));
    }

    #endregion

    #region CloseAsync Tests

    [Fact]
    public async Task CloseAsync_WithAuthor_ShouldCloseQuestion()
    {
        // Arrange
        var question = new Question
        {
            Title = "종료 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            Status = QuestionStatus.Open
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CloseAsync(question.Id, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(QuestionStatus.Closed);
    }

    [Fact]
    public async Task CloseAsync_WithNonAuthor_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var question = new Question
        {
            Title = "종료 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.CloseAsync(question.Id, userId: 2));
    }

    #endregion

    #region ReopenAsync Tests

    [Fact]
    public async Task ReopenAsync_WithAuthor_ShouldReopenQuestion()
    {
        // Arrange
        var question = new Question
        {
            Title = "다시 열기 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            Status = QuestionStatus.Closed
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ReopenAsync(question.Id, userId: 1);

        // Assert
        result.Status.Should().Be(QuestionStatus.Open);
    }

    #endregion

    #region VoteAsync Tests

    [Fact]
    public async Task VoteAsync_WithUpvote_ShouldIncreaseVoteCount()
    {
        // Arrange
        var question = new Question
        {
            Title = "투표 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            VoteCount = 0
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoteAsync(question.Id, userId: 2, VoteType.Up);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task VoteAsync_WithDownvote_ShouldDecreaseVoteCount()
    {
        // Arrange
        var question = new Question
        {
            Title = "투표 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            VoteCount = 5
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoteAsync(question.Id, userId: 2, VoteType.Down);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public async Task VoteAsync_OnOwnQuestion_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var question = new Question
        {
            Title = "투표 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VoteAsync(question.Id, userId: 1, VoteType.Up));
    }

    #endregion

    #region RemoveVoteAsync Tests

    [Fact]
    public async Task RemoveVoteAsync_WithExistingVote_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var question = new Question
        {
            Title = "투표 취소 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            VoteCount = 1
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        _context.QuestionVotes.Add(new QuestionVote
        {
            QuestionId = question.Id,
            UserId = 2,
            VoteType = VoteType.Up
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVoteAsync(question.Id, userId: 2);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(question).ReloadAsync();
        question.VoteCount.Should().Be(0);
    }

    [Fact]
    public async Task RemoveVoteAsync_WithNoExistingVote_ShouldReturnFalse()
    {
        // Arrange
        var question = new Question
        {
            Title = "투표 취소 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVoteAsync(question.Id, userId: 2);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IncrementViewCountAsync Tests

    [Fact]
    public async Task IncrementViewCountAsync_ShouldIncreaseViewCount()
    {
        // Arrange
        var question = new Question
        {
            Title = "조회수 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            ViewCount = 10
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        await _service.IncrementViewCountAsync(question.Id);

        // Assert
        await _context.Entry(question).ReloadAsync();
        question.ViewCount.Should().Be(11);
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public async Task ExistsAsync_WithExistingQuestion_ShouldReturnTrue()
    {
        // Arrange
        var question = new Question
        {
            Title = "존재 확인",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ExistsAsync(question.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingQuestion_ShouldReturnFalse()
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
        var question = new Question
        {
            Title = "작성자 확인",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsAuthorAsync(question.Id, userId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthorAsync_WithNonAuthor_ShouldReturnFalse()
    {
        // Arrange
        var question = new Question
        {
            Title = "작성자 확인",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsAuthorAsync(question.Id, userId: 2);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
