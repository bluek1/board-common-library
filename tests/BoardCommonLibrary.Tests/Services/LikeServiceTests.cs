using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// LikeService 단위 테스트
/// </summary>
public class LikeServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly LikeService _service;
    
    public LikeServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BoardDbContext(options);
        _service = new LikeService(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var post = new Post
        {
            Id = 1,
            Title = "테스트 게시물",
            Content = "테스트 내용",
            AuthorId = 1,
            Status = PostStatus.Published,
            CreatedAt = DateTime.UtcNow
        };
        
        var comment = new Comment
        {
            Id = 1,
            Content = "테스트 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        _context.Comments.Add(comment);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    #region LikePostAsync Tests
    
    [Fact]
    public async Task LikePostAsync_ValidRequest_ReturnsLikeResponse()
    {
        // Act
        var result = await _service.LikePostAsync(1, 2);
        
        // Assert
        result.Should().NotBeNull();
        result.IsLiked.Should().BeTrue();
        result.TotalLikeCount.Should().Be(1);
    }
    
    [Fact]
    public async Task LikePostAsync_IncrementsPostLikeCount()
    {
        // Act
        await _service.LikePostAsync(1, 2);
        
        // Assert
        var post = await _context.Posts.FindAsync(1L);
        post!.LikeCount.Should().Be(1);
    }
    
    [Fact]
    public async Task LikePostAsync_DuplicateLike_ThrowsException()
    {
        // Arrange
        await _service.LikePostAsync(1, 2);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LikePostAsync(1, 2));
    }
    
    [Fact]
    public async Task LikePostAsync_NonExistingPost_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LikePostAsync(999, 1));
    }
    
    #endregion
    
    #region UnlikePostAsync Tests
    
    [Fact]
    public async Task UnlikePostAsync_ValidRequest_ReturnsLikeResponse()
    {
        // Arrange
        await _service.LikePostAsync(1, 2);
        
        // Act
        var result = await _service.UnlikePostAsync(1, 2);
        
        // Assert
        result.Should().NotBeNull();
        result!.IsLiked.Should().BeFalse();
        result.TotalLikeCount.Should().Be(0);
    }
    
    [Fact]
    public async Task UnlikePostAsync_DecrementsPostLikeCount()
    {
        // Arrange
        await _service.LikePostAsync(1, 2);
        
        // Act
        await _service.UnlikePostAsync(1, 2);
        
        // Assert
        var post = await _context.Posts.FindAsync(1L);
        post!.LikeCount.Should().Be(0);
    }
    
    [Fact]
    public async Task UnlikePostAsync_NotLiked_ReturnsNull()
    {
        // Act
        var result = await _service.UnlikePostAsync(1, 2);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task UnlikePostAsync_NonExistingPost_ReturnsNull()
    {
        // Act
        var result = await _service.UnlikePostAsync(999, 1);
        
        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    #region LikeCommentAsync Tests
    
    [Fact]
    public async Task LikeCommentAsync_ValidRequest_ReturnsLikeResponse()
    {
        // Act
        var result = await _service.LikeCommentAsync(1, 3);
        
        // Assert
        result.Should().NotBeNull();
        result.IsLiked.Should().BeTrue();
        result.TotalLikeCount.Should().Be(1);
    }
    
    [Fact]
    public async Task LikeCommentAsync_IncrementsCommentLikeCount()
    {
        // Act
        await _service.LikeCommentAsync(1, 3);
        
        // Assert
        var comment = await _context.Comments.FindAsync(1L);
        comment!.LikeCount.Should().Be(1);
    }
    
    [Fact]
    public async Task LikeCommentAsync_DuplicateLike_ThrowsException()
    {
        // Arrange
        await _service.LikeCommentAsync(1, 3);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LikeCommentAsync(1, 3));
    }
    
    [Fact]
    public async Task LikeCommentAsync_NonExistingComment_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LikeCommentAsync(999, 1));
    }
    
    #endregion
    
    #region UnlikeCommentAsync Tests
    
    [Fact]
    public async Task UnlikeCommentAsync_ValidRequest_ReturnsLikeResponse()
    {
        // Arrange
        await _service.LikeCommentAsync(1, 3);
        
        // Act
        var result = await _service.UnlikeCommentAsync(1, 3);
        
        // Assert
        result.Should().NotBeNull();
        result!.IsLiked.Should().BeFalse();
        result.TotalLikeCount.Should().Be(0);
    }
    
    [Fact]
    public async Task UnlikeCommentAsync_DecrementsCommentLikeCount()
    {
        // Arrange
        await _service.LikeCommentAsync(1, 3);
        
        // Act
        await _service.UnlikeCommentAsync(1, 3);
        
        // Assert
        var comment = await _context.Comments.FindAsync(1L);
        comment!.LikeCount.Should().Be(0);
    }
    
    [Fact]
    public async Task UnlikeCommentAsync_NotLiked_ReturnsNull()
    {
        // Act
        var result = await _service.UnlikeCommentAsync(1, 3);
        
        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    #region HasUserLikedPostAsync Tests
    
    [Fact]
    public async Task HasUserLikedPostAsync_Liked_ReturnsTrue()
    {
        // Arrange
        await _service.LikePostAsync(1, 2);
        
        // Act
        var result = await _service.HasUserLikedPostAsync(1, 2);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task HasUserLikedPostAsync_NotLiked_ReturnsFalse()
    {
        // Act
        var result = await _service.HasUserLikedPostAsync(1, 2);
        
        // Assert
        result.Should().BeFalse();
    }
    
    #endregion
    
    #region HasUserLikedCommentAsync Tests
    
    [Fact]
    public async Task HasUserLikedCommentAsync_Liked_ReturnsTrue()
    {
        // Arrange
        await _service.LikeCommentAsync(1, 3);
        
        // Act
        var result = await _service.HasUserLikedCommentAsync(1, 3);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task HasUserLikedCommentAsync_NotLiked_ReturnsFalse()
    {
        // Act
        var result = await _service.HasUserLikedCommentAsync(1, 3);
        
        // Assert
        result.Should().BeFalse();
    }
    
    #endregion
}
