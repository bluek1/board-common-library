using BoardCommonLibrary.Data;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// ViewCountService 단위 테스트
/// </summary>
public class ViewCountServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly ViewCountService _viewCountService;

    public ViewCountServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _viewCountService = new ViewCountService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Post> CreateTestPost()
    {
        var post = new Post
        {
            Title = "조회수 테스트",
            Content = "내용",
            AuthorId = 1,
            ViewCount = 0
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    #region IncrementViewCount Tests

    [Fact]
    public async Task IncrementViewCountAsync_WithNewUser_ShouldIncrementAndReturnTrue()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        var result = await _viewCountService.IncrementViewCountAsync(post.Id, userId: 1, ipAddress: null);

        // Assert
        result.Should().BeTrue();

        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_WithAnonymousUser_ShouldIncrementAndReturnTrue()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        var result = await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: "192.168.1.1");

        // Assert
        result.Should().BeTrue();

        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_SameUserWithin24Hours_ShouldNotIncrementAndReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();

        // 첫 번째 조회
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: 1, ipAddress: null);

        // Act - 같은 사용자가 다시 조회
        var result = await _viewCountService.IncrementViewCountAsync(post.Id, userId: 1, ipAddress: null);

        // Assert
        result.Should().BeFalse();

        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(1); // 여전히 1
    }

    [Fact]
    public async Task IncrementViewCountAsync_SameIPWithin24Hours_ShouldNotIncrementAndReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();
        var ipAddress = "192.168.1.100";

        // 첫 번째 조회
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: ipAddress);

        // Act - 같은 IP가 다시 조회
        var result = await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: ipAddress);

        // Assert
        result.Should().BeFalse();

        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_DifferentUsers_ShouldIncrementSeparately()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: 1, ipAddress: null);
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: 2, ipAddress: null);
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: 3, ipAddress: null);

        // Assert
        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(3);
    }

    [Fact]
    public async Task IncrementViewCountAsync_DifferentIPs_ShouldIncrementSeparately()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: "192.168.1.1");
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: "192.168.1.2");
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: null, ipAddress: "192.168.1.3");

        // Assert
        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost!.ViewCount.Should().Be(3);
    }

    [Fact]
    public async Task IncrementViewCountAsync_SameUserDifferentPosts_ShouldIncrementEach()
    {
        // Arrange
        var post1 = await CreateTestPost();
        var post2 = new Post { Title = "두 번째", Content = "내용", AuthorId = 1, ViewCount = 0 };
        _context.Posts.Add(post2);
        await _context.SaveChangesAsync();

        // Act
        await _viewCountService.IncrementViewCountAsync(post1.Id, userId: 1, ipAddress: null);
        await _viewCountService.IncrementViewCountAsync(post2.Id, userId: 1, ipAddress: null);

        // Assert
        var updatedPost1 = await _context.Posts.FindAsync(post1.Id);
        var updatedPost2 = await _context.Posts.FindAsync(post2.Id);

        updatedPost1!.ViewCount.Should().Be(1);
        updatedPost2!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task IncrementViewCountAsync_ShouldCreateViewRecord()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        await _viewCountService.IncrementViewCountAsync(post.Id, userId: 1, ipAddress: "192.168.1.1");

        // Assert
        var viewRecord = await _context.ViewRecords.FirstOrDefaultAsync(v => v.PostId == post.Id);
        viewRecord.Should().NotBeNull();
        viewRecord!.UserId.Should().Be(1);
        viewRecord.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task IncrementViewCountAsync_WithNonExistingPost_ShouldReturnTrueButNotFail()
    {
        // ViewRecord는 생성되지만 Post가 없어도 예외는 발생하지 않음
        // Act
        var result = await _viewCountService.IncrementViewCountAsync(999, userId: 1, ipAddress: null);

        // Assert - ViewRecord는 생성됨
        result.Should().BeTrue();
    }

    #endregion

    #region GetViewCount Tests

    [Fact]
    public async Task GetViewCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var post = new Post
        {
            Title = "조회수 테스트",
            Content = "내용",
            AuthorId = 1,
            ViewCount = 42
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.GetViewCountAsync(post.Id);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task GetViewCountAsync_WithNonExistingPost_ShouldReturnZero()
    {
        // Act
        var result = await _viewCountService.GetViewCountAsync(999);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region HasViewed Tests

    [Fact]
    public async Task HasViewedAsync_WithNoRecord_ShouldReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: 1, ipAddress: null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasViewedAsync_WithRecentUserRecord_ShouldReturnTrue()
    {
        // Arrange
        var post = await CreateTestPost();
        _context.ViewRecords.Add(new ViewRecord
        {
            PostId = post.Id,
            UserId = 1,
            ViewedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: 1, ipAddress: null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasViewedAsync_WithRecentIPRecord_ShouldReturnTrue()
    {
        // Arrange
        var post = await CreateTestPost();
        _context.ViewRecords.Add(new ViewRecord
        {
            PostId = post.Id,
            IpAddress = "192.168.1.1",
            ViewedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: null, ipAddress: "192.168.1.1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasViewedAsync_WithOldRecord_ShouldReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();
        _context.ViewRecords.Add(new ViewRecord
        {
            PostId = post.Id,
            UserId = 1,
            ViewedAt = DateTime.UtcNow.AddHours(-25) // 25시간 전
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: 1, ipAddress: null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasViewedAsync_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();
        _context.ViewRecords.Add(new ViewRecord
        {
            PostId = post.Id,
            UserId = 1,
            ViewedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: 2, ipAddress: null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasViewedAsync_WithDifferentIP_ShouldReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();
        _context.ViewRecords.Add(new ViewRecord
        {
            PostId = post.Id,
            IpAddress = "192.168.1.1",
            ViewedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: null, ipAddress: "192.168.1.2");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasViewedAsync_WithNullUserAndIP_ShouldReturnFalse()
    {
        // Arrange
        var post = await CreateTestPost();

        // Act
        var result = await _viewCountService.HasViewedAsync(post.Id, userId: null, ipAddress: null);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
