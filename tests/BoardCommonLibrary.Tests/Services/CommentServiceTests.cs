using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// CommentService 단위 테스트
/// </summary>
public class CommentServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly CommentService _service;
    
    public CommentServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BoardDbContext(options);
        _service = new CommentService(_context);
        
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
            AuthorName = "작성자1",
            Status = PostStatus.Published,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Posts.Add(post);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    #region CreateAsync Tests
    
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCommentResponse()
    {
        // Arrange
        var request = new CreateCommentRequest { Content = "테스트 댓글입니다." };
        
        // Act
        var result = await _service.CreateAsync(1, request, 2, "작성자2");
        
        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("테스트 댓글입니다.");
        result.PostId.Should().Be(1);
        result.AuthorId.Should().Be(2);
        result.AuthorName.Should().Be("작성자2");
    }
    
    [Fact]
    public async Task CreateAsync_InvalidPostId_ThrowsException()
    {
        // Arrange
        var request = new CreateCommentRequest { Content = "테스트 댓글" };
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(999, request, 1, "작성자"));
    }
    
    [Fact]
    public async Task CreateAsync_IncrementsPostCommentCount()
    {
        // Arrange
        var request = new CreateCommentRequest { Content = "테스트 댓글" };
        var initialCount = (await _context.Posts.FindAsync(1L))!.CommentCount;
        
        // Act
        await _service.CreateAsync(1, request, 2, "작성자2");
        
        // Assert
        var post = await _context.Posts.FindAsync(1L);
        post!.CommentCount.Should().Be(initialCount + 1);
    }
    
    #endregion
    
    #region GetByPostIdAsync Tests
    
    [Fact]
    public async Task GetByPostIdAsync_ReturnsCommentsForPost()
    {
        // Arrange
        await CreateTestComments();
        var parameters = new CommentQueryParameters { Page = 1, PageSize = 10 };
        
        // Act
        var result = await _service.GetByPostIdAsync(1, parameters);
        
        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetByPostIdAsync_SortsByCreatedAtAscByDefault()
    {
        // Arrange
        await CreateTestComments();
        var parameters = new CommentQueryParameters { Page = 1, PageSize = 10, SortOrder = "asc" };
        
        // Act
        var result = await _service.GetByPostIdAsync(1, parameters);
        
        // Assert
        result.Data[0].Content.Should().Be("첫 번째 댓글");
        result.Data[1].Content.Should().Be("두 번째 댓글");
    }
    
    [Fact]
    public async Task GetByPostIdAsync_IncludesReplies()
    {
        // Arrange
        await CreateTestComments();
        var parentComment = await _context.Comments.FirstAsync(c => c.ParentId == null);
        
        var reply = new Comment
        {
            Content = "대댓글입니다",
            PostId = 1,
            AuthorId = 3,
            ParentId = parentComment.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(reply);
        await _context.SaveChangesAsync();
        
        var parameters = new CommentQueryParameters { Page = 1, PageSize = 10, IncludeReplies = true };
        
        // Act
        var result = await _service.GetByPostIdAsync(1, parameters);
        
        // Assert
        var firstComment = result.Data.First(c => c.Id == parentComment.Id);
        firstComment.Replies.Should().NotBeNull();
        firstComment.Replies.Should().HaveCount(1);
    }
    
    #endregion
    
    #region UpdateAsync Tests
    
    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedComment()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "원본 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        var request = new UpdateCommentRequest { Content = "수정된 댓글" };
        
        // Act
        var result = await _service.UpdateAsync(comment.Id, request, 2);
        
        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("수정된 댓글");
    }
    
    [Fact]
    public async Task UpdateAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "원본 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        var request = new UpdateCommentRequest { Content = "수정된 댓글" };
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(comment.Id, request, 999));
    }
    
    [Fact]
    public async Task UpdateAsync_NonExistingComment_ReturnsNull()
    {
        // Arrange
        var request = new UpdateCommentRequest { Content = "수정된 댓글" };
        
        // Act
        var result = await _service.UpdateAsync(999, request, 1);
        
        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    #region DeleteAsync Tests
    
    [Fact]
    public async Task DeleteAsync_ValidRequest_SoftDeletesComment()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "삭제할 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DeleteAsync(comment.Id, 2);
        
        // Assert
        result.Should().BeTrue();
        var deletedComment = await _context.Comments.FindAsync(comment.Id);
        deletedComment!.IsDeleted.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteAsync_WithReplies_KeepsStructure()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "삭제할 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        var reply = new Comment
        {
            Content = "대댓글",
            PostId = 1,
            AuthorId = 3,
            ParentId = comment.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(reply);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DeleteAsync(comment.Id, 2);
        
        // Assert
        result.Should().BeTrue();
        var deletedComment = await _context.Comments.FindAsync(comment.Id);
        deletedComment!.IsDeleted.Should().BeTrue();
        deletedComment.Content.Should().Be("삭제된 댓글입니다.");
    }
    
    [Fact]
    public async Task DeleteAsync_NotAuthorNotAdmin_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "삭제할 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteAsync(comment.Id, 999, false));
    }
    
    [Fact]
    public async Task DeleteAsync_AdminCanDelete()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "삭제할 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DeleteAsync(comment.Id, 999, isAdmin: true);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteAsync_DecrementsPostCommentCount()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "삭제할 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        
        var post = await _context.Posts.FindAsync(1L);
        post!.CommentCount = 5;
        await _context.SaveChangesAsync();
        
        // Act
        await _service.DeleteAsync(comment.Id, 2);
        
        // Assert
        post = await _context.Posts.FindAsync(1L);
        post!.CommentCount.Should().Be(4);
    }
    
    #endregion
    
    #region CreateReplyAsync Tests
    
    [Fact]
    public async Task CreateReplyAsync_ValidRequest_ReturnsReply()
    {
        // Arrange
        var parentComment = new Comment
        {
            Content = "부모 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(parentComment);
        await _context.SaveChangesAsync();
        
        var request = new CreateCommentRequest { Content = "대댓글입니다" };
        
        // Act
        var result = await _service.CreateReplyAsync(parentComment.Id, request, 3, "작성자3");
        
        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("대댓글입니다");
        result.ParentId.Should().Be(parentComment.Id);
        result.PostId.Should().Be(1);
    }
    
    [Fact]
    public async Task CreateReplyAsync_NestedReply_ThrowsException()
    {
        // Arrange
        var parentComment = new Comment
        {
            Content = "부모 댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(parentComment);
        await _context.SaveChangesAsync();
        
        var reply = new Comment
        {
            Content = "대댓글",
            PostId = 1,
            AuthorId = 3,
            ParentId = parentComment.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(reply);
        await _context.SaveChangesAsync();
        
        var request = new CreateCommentRequest { Content = "대대댓글 시도" };
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateReplyAsync(reply.Id, request, 4, "작성자4"));
    }
    
    [Fact]
    public async Task CreateReplyAsync_NonExistingParent_ReturnsNull()
    {
        // Arrange
        var request = new CreateCommentRequest { Content = "대댓글" };
        
        // Act
        var result = await _service.CreateReplyAsync(999, request, 1, "작성자");
        
        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    #region IsAuthorAsync Tests
    
    [Fact]
    public async Task IsAuthorAsync_IsAuthor_ReturnsTrue()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.IsAuthorAsync(comment.Id, 2);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsAuthorAsync_NotAuthor_ReturnsFalse()
    {
        // Arrange
        var comment = new Comment
        {
            Content = "댓글",
            PostId = 1,
            AuthorId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.IsAuthorAsync(comment.Id, 999);
        
        // Assert
        result.Should().BeFalse();
    }
    
    #endregion
    
    private async Task CreateTestComments()
    {
        var comments = new List<Comment>
        {
            new()
            {
                Content = "첫 번째 댓글",
                PostId = 1,
                AuthorId = 2,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                Content = "두 번째 댓글",
                PostId = 1,
                AuthorId = 3,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };
        
        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();
    }
}
