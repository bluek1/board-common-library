using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// BookmarkService 단위 테스트
/// </summary>
public class BookmarkServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly BookmarkService _service;
    
    public BookmarkServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BoardDbContext(options);
        _service = new BookmarkService(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var posts = new List<Post>
        {
            new Post
            {
                Id = 1,
                Title = "테스트 게시물 1",
                Content = "테스트 내용 1",
                AuthorId = 1,
                Status = PostStatus.Published,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Post
            {
                Id = 2,
                Title = "테스트 게시물 2",
                Content = "테스트 내용 2",
                AuthorId = 1,
                Status = PostStatus.Published,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Post
            {
                Id = 3,
                Title = "테스트 게시물 3",
                Content = "테스트 내용 3",
                AuthorId = 2,
                Status = PostStatus.Published,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _context.Posts.AddRange(posts);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    #region AddBookmarkAsync Tests
    
    [Fact]
    public async Task AddBookmarkAsync_ValidRequest_ReturnsTrue()
    {
        // Act
        var result = await _service.AddBookmarkAsync(1, 2);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task AddBookmarkAsync_DuplicateBookmark_ThrowsException()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddBookmarkAsync(1, 2));
    }
    
    [Fact]
    public async Task AddBookmarkAsync_NonExistingPost_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddBookmarkAsync(999, 1));
    }
    
    [Fact]
    public async Task AddBookmarkAsync_SavesBookmarkToDatabase()
    {
        // Act
        await _service.AddBookmarkAsync(1, 2);
        
        // Assert
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.PostId == 1 && b.UserId == 2);
        bookmark.Should().NotBeNull();
    }
    
    #endregion
    
    #region RemoveBookmarkAsync Tests
    
    [Fact]
    public async Task RemoveBookmarkAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        
        // Act
        var result = await _service.RemoveBookmarkAsync(1, 2);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task RemoveBookmarkAsync_RemovesFromDatabase()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        
        // Act
        await _service.RemoveBookmarkAsync(1, 2);
        
        // Assert
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.PostId == 1 && b.UserId == 2);
        bookmark.Should().BeNull();
    }
    
    [Fact]
    public async Task RemoveBookmarkAsync_NotBookmarked_ReturnsFalse()
    {
        // Act
        var result = await _service.RemoveBookmarkAsync(1, 2);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task RemoveBookmarkAsync_NonExistingPost_ReturnsFalse()
    {
        // Act
        var result = await _service.RemoveBookmarkAsync(999, 1);
        
        // Assert
        result.Should().BeFalse();
    }
    
    #endregion
    
    #region GetUserBookmarksAsync Tests
    
    [Fact]
    public async Task GetUserBookmarksAsync_ReturnsBookmarkedPosts()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        await _service.AddBookmarkAsync(2, 2);
        
        var parameters = new BookmarkQueryParameters { Page = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetUserBookmarksAsync(2, parameters);
        
        // Assert
        result.Should().NotBeNull();
        result.Meta.TotalCount.Should().Be(2);
        result.Data.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetUserBookmarksAsync_ReturnsPaginatedResults()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        await _service.AddBookmarkAsync(2, 2);
        await _service.AddBookmarkAsync(3, 2);
        
        var parameters = new BookmarkQueryParameters { Page = 1, PageSize = 2 };
        
        // Act
        var result = await _service.GetUserBookmarksAsync(2, parameters);
        
        // Assert
        result.Should().NotBeNull();
        result.Meta.TotalCount.Should().Be(3);
        result.Data.Should().HaveCount(2);
        result.Meta.TotalPages.Should().Be(2);
    }
    
    [Fact]
    public async Task GetUserBookmarksAsync_ReturnsEmptyForNoBookmarks()
    {
        // Arrange
        var parameters = new BookmarkQueryParameters { Page = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetUserBookmarksAsync(99, parameters);
        
        // Assert
        result.Should().NotBeNull();
        result.Meta.TotalCount.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetUserBookmarksAsync_OrderedByCreatedAtDescending()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        await Task.Delay(10); // 작은 지연 추가
        await _service.AddBookmarkAsync(2, 2);
        await Task.Delay(10);
        await _service.AddBookmarkAsync(3, 2);
        
        var parameters = new BookmarkQueryParameters { Page = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetUserBookmarksAsync(2, parameters);
        
        // Assert
        result.Data.Should().HaveCount(3);
        // 가장 최근 북마크가 먼저 (PostId 3)
        result.Data.First().PostId.Should().Be(3);
    }
    
    [Fact]
    public async Task GetUserBookmarksAsync_DoesNotReturnOtherUsersBookmarks()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        await _service.AddBookmarkAsync(2, 3); // 다른 사용자의 북마크
        
        var parameters = new BookmarkQueryParameters { Page = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetUserBookmarksAsync(2, parameters);
        
        // Assert
        result.Meta.TotalCount.Should().Be(1);
        // BookmarkResponse에는 UserId가 없으므로 PostId로 검증
        result.Data.Should().HaveCount(1);
        result.Data.First().PostId.Should().Be(1);
    }
    
    #endregion
    
    #region HasUserBookmarkedAsync Tests
    
    [Fact]
    public async Task HasUserBookmarkedAsync_Bookmarked_ReturnsTrue()
    {
        // Arrange
        await _service.AddBookmarkAsync(1, 2);
        
        // Act
        var result = await _service.HasUserBookmarkedAsync(1, 2);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task HasUserBookmarkedAsync_NotBookmarked_ReturnsFalse()
    {
        // Act
        var result = await _service.HasUserBookmarkedAsync(1, 2);
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task HasUserBookmarkedAsync_NonExistingPost_ReturnsFalse()
    {
        // Act
        var result = await _service.HasUserBookmarkedAsync(999, 1);
        
        // Assert
        result.Should().BeFalse();
    }
    
    #endregion
}
