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
/// AdminService 단위 테스트
/// </summary>
public class AdminServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly AdminService _service;
    private readonly Mock<ILogger<AdminService>> _loggerMock;

    public AdminServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _loggerMock = new Mock<ILogger<AdminService>>();
        _service = new AdminService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllPostsAsync Tests

    [Fact]
    public async Task GetAllPostsAsync_WithPosts_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.Posts.Add(new Post
            {
                Title = $"게시물 {i + 1}",
                Content = $"내용 {i + 1}",
                AuthorId = 1,
                AuthorName = "작성자"
            });
        }
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Meta.TotalCount.Should().Be(15);
        result.Meta.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetAllPostsAsync_WithDeletedPosts_ShouldIncludeDeletedPosts()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "일반 게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsDeleted = false },
            new Post { Title = "삭제된 게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters();

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllPostsAsync_FilterByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "발행됨", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = PostStatus.Published },
            new Post { Title = "초안", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = PostStatus.Draft },
            new Post { Title = "발행됨 2", Content = "내용", AuthorId = 1, AuthorName = "작성자", Status = PostStatus.Published }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters { Status = PostStatus.Published };

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(p => p.Status == PostStatus.Published);
    }

    [Fact]
    public async Task GetAllPostsAsync_FilterByIsDeleted_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "일반", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsDeleted = false },
            new Post { Title = "삭제됨", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsDeleted = true },
            new Post { Title = "일반 2", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters { IsDeleted = true };

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllPostsAsync_FilterByIsBlinded_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "일반", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsBlinded = false },
            new Post { Title = "블라인드", Content = "내용", AuthorId = 1, AuthorName = "작성자", IsBlinded = true }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters { IsBlinded = true };

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllPostsAsync_FilterByQuery_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "테스트 제목", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Post { Title = "다른 제목", Content = "테스트 내용", AuthorId = 1, AuthorName = "작성자" },
            new Post { Title = "무관한 게시물", Content = "무관한 내용", AuthorId = 1, AuthorName = "작성자" }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminPostQueryParameters { Query = "테스트" };

        // Act
        var result = await _service.GetAllPostsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
    }

    #endregion

    #region GetAllCommentsAsync Tests

    [Fact]
    public async Task GetAllCommentsAsync_WithComments_ShouldReturnPagedResults()
    {
        // Arrange
        var post = new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        for (int i = 0; i < 15; i++)
        {
            _context.Comments.Add(new Comment
            {
                Content = $"댓글 {i + 1}",
                PostId = post.Id,
                AuthorId = 1,
                AuthorName = "작성자"
            });
        }
        await _context.SaveChangesAsync();

        var parameters = new AdminCommentQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllCommentsAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Meta.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetAllCommentsAsync_FilterByPostId_ShouldReturnFilteredResults()
    {
        // Arrange
        var post1 = new Post { Title = "게시물 1", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        var post2 = new Post { Title = "게시물 2", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.AddRange(post1, post2);
        await _context.SaveChangesAsync();

        _context.Comments.AddRange(
            new Comment { Content = "댓글 1", PostId = post1.Id, AuthorId = 1, AuthorName = "작성자" },
            new Comment { Content = "댓글 2", PostId = post1.Id, AuthorId = 1, AuthorName = "작성자" },
            new Comment { Content = "댓글 3", PostId = post2.Id, AuthorId = 1, AuthorName = "작성자" }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminCommentQueryParameters { PostId = post1.Id };

        // Act
        var result = await _service.GetAllCommentsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllCommentsAsync_FilterByIsBlinded_ShouldReturnFilteredResults()
    {
        // Arrange
        var post = new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _context.Comments.AddRange(
            new Comment { Content = "일반 댓글", PostId = post.Id, AuthorId = 1, AuthorName = "작성자", IsBlinded = false },
            new Comment { Content = "블라인드 댓글", PostId = post.Id, AuthorId = 1, AuthorName = "작성자", IsBlinded = true }
        );
        await _context.SaveChangesAsync();

        var parameters = new AdminCommentQueryParameters { IsBlinded = true };

        // Act
        var result = await _service.GetAllCommentsAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
    }

    #endregion

    #region BatchDeleteAsync Tests

    [Fact]
    public async Task BatchDeleteAsync_PostsSoftDelete_ShouldDeletePosts()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "게시물 1", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Post { Title = "게시물 2", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Post { Title = "게시물 3", Content = "내용", AuthorId = 1, AuthorName = "작성자" }
        );
        await _context.SaveChangesAsync();

        var posts = await _context.Posts.ToListAsync();
        var request = new BatchDeleteRequest
        {
            TargetType = BatchTargetType.Post,
            Ids = posts.Select(p => p.Id).ToList(),
            HardDelete = false
        };

        // Act
        var result = await _service.BatchDeleteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SuccessCount.Should().Be(3);
        result.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task BatchDeleteAsync_WithNonExistingIds_ShouldReportFailures()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" });
        await _context.SaveChangesAsync();

        var post = await _context.Posts.FirstAsync();
        var request = new BatchDeleteRequest
        {
            TargetType = BatchTargetType.Post,
            Ids = new List<long> { post.Id, 999, 1000 },
            HardDelete = false
        };

        // Act
        var result = await _service.BatchDeleteAsync(request);

        // Assert
        result.SuccessCount.Should().Be(1);
        result.FailedCount.Should().Be(2);
        result.FailedIds.Should().Contain(999);
        result.FailedIds.Should().Contain(1000);
    }

    [Fact]
    public async Task BatchDeleteAsync_CommentsSoftDelete_ShouldDeleteComments()
    {
        // Arrange
        var post = new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _context.Comments.AddRange(
            new Comment { Content = "댓글 1", PostId = post.Id, AuthorId = 1, AuthorName = "작성자" },
            new Comment { Content = "댓글 2", PostId = post.Id, AuthorId = 1, AuthorName = "작성자" }
        );
        await _context.SaveChangesAsync();

        var comments = await _context.Comments.ToListAsync();
        var request = new BatchDeleteRequest
        {
            TargetType = BatchTargetType.Comment,
            Ids = comments.Select(c => c.Id).ToList(),
            HardDelete = false
        };

        // Act
        var result = await _service.BatchDeleteAsync(request);

        // Assert
        result.SuccessCount.Should().Be(2);
        result.FailedCount.Should().Be(0);
    }

    #endregion

    #region BlindContentAsync Tests

    [Fact]
    public async Task BlindContentAsync_Post_ShouldBlindPost()
    {
        // Arrange
        var post = new Post
        {
            Title = "블라인드 대상",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            IsBlinded = false
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new BlindContentRequest
        {
            TargetType = BatchTargetType.Post,
            TargetId = post.Id,
            IsBlinded = true,
            Reason = "부적절한 콘텐츠"
        };

        // Act
        var result = await _service.BlindContentAsync(request);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(post).ReloadAsync();
        post.IsBlinded.Should().BeTrue();
    }

    [Fact]
    public async Task BlindContentAsync_Comment_ShouldBlindComment()
    {
        // Arrange
        var post = new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var comment = new Comment
        {
            Content = "블라인드 대상 댓글",
            PostId = post.Id,
            AuthorId = 1,
            AuthorName = "작성자",
            IsBlinded = false
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var request = new BlindContentRequest
        {
            TargetType = BatchTargetType.Comment,
            TargetId = comment.Id,
            IsBlinded = true
        };

        // Act
        var result = await _service.BlindContentAsync(request);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(comment).ReloadAsync();
        comment.IsBlinded.Should().BeTrue();
    }

    [Fact]
    public async Task BlindContentAsync_UnblindPost_ShouldUnblindPost()
    {
        // Arrange
        var post = new Post
        {
            Title = "블라인드 해제 대상",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            IsBlinded = true
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new BlindContentRequest
        {
            TargetType = BatchTargetType.Post,
            TargetId = post.Id,
            IsBlinded = false
        };

        // Act
        var result = await _service.BlindContentAsync(request);

        // Assert
        result.Should().BeTrue();
        await _context.Entry(post).ReloadAsync();
        post.IsBlinded.Should().BeFalse();
    }

    [Fact]
    public async Task BlindContentAsync_NonExistingTarget_ShouldReturnFalse()
    {
        // Arrange
        var request = new BlindContentRequest
        {
            TargetType = BatchTargetType.Post,
            TargetId = 999,
            IsBlinded = true
        };

        // Act
        var result = await _service.BlindContentAsync(request);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_WithData_ShouldReturnStatistics()
    {
        // Arrange
        _context.Posts.AddRange(
            new Post { Title = "게시물 1", Content = "내용", AuthorId = 1, AuthorName = "작성자" },
            new Post { Title = "게시물 2", Content = "내용", AuthorId = 1, AuthorName = "작성자" }
        );
        _context.Questions.Add(new Question
        {
            Title = "질문",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalPosts.Should().Be(2);
        result.TotalQuestions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithNoData_ShouldReturnZeroStatistics()
    {
        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalPosts.Should().Be(0);
        result.TotalComments.Should().Be(0);
        result.TotalQuestions.Should().Be(0);
        result.TotalAnswers.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludeTodayStatistics()
    {
        // Arrange
        _context.Posts.Add(new Post
        {
            Title = "오늘 게시물",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자",
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TodayPosts.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludePendingReports()
    {
        // Arrange
        var post = new Post { Title = "게시물", Content = "내용", AuthorId = 1, AuthorName = "작성자" };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = post.Id, ReporterId = 2, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = post.Id, ReporterId = 3, ReporterName = "신고자2", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = post.Id, ReporterId = 4, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Approved }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.PendingReports.Should().Be(2);
        result.TotalReports.Should().Be(3);
    }

    #endregion
}
