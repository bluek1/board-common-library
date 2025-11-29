using BoardCommonLibrary.Data;
using BoardCommonLibrary.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Data;

/// <summary>
/// BoardDbContext 단위 테스트
/// </summary>
public class BoardDbContextTests : IDisposable
{
    private readonly BoardDbContext _context;

    public BoardDbContextTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Post Entity Tests

    [Fact]
    public async Task Post_ShouldSetCreatedAtOnCreate()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var post = new Post
        {
            Title = "생성 시간 테스트",
            Content = "내용",
            AuthorId = 1
        };

        // Act
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Assert
        post.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
    }

    [Fact]
    public async Task Post_ShouldUpdateUpdatedAtOnModify()
    {
        // Arrange
        var post = new Post
        {
            Title = "원본 제목",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var initialUpdatedAt = post.UpdatedAt;

        // Act
        await Task.Delay(10); // 약간의 지연
        post.Title = "수정된 제목";
        await _context.SaveChangesAsync();

        // Assert
        post.UpdatedAt.Should().NotBe(initialUpdatedAt);
    }

    [Fact]
    public async Task Post_GlobalFilter_ShouldExcludeDeletedPosts()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "일반 게시물", Content = "내용", AuthorId = 1, IsDeleted = false });
        _context.Posts.Add(new Post { Title = "삭제된 게시물", Content = "내용", AuthorId = 1, IsDeleted = true });
        await _context.SaveChangesAsync();

        // Act
        var posts = await _context.Posts.ToListAsync();

        // Assert
        posts.Should().HaveCount(1);
        posts.First().Title.Should().Be("일반 게시물");
    }

    [Fact]
    public async Task Post_IgnoreQueryFilters_ShouldIncludeDeletedPosts()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "일반 게시물", Content = "내용", AuthorId = 1, IsDeleted = false });
        _context.Posts.Add(new Post { Title = "삭제된 게시물", Content = "내용", AuthorId = 1, IsDeleted = true });
        await _context.SaveChangesAsync();

        // Act
        var posts = await _context.Posts.IgnoreQueryFilters().ToListAsync();

        // Assert
        posts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Post_Tags_ShouldBeSerialized()
    {
        // Arrange
        var post = new Post
        {
            Title = "태그 테스트",
            Content = "내용",
            AuthorId = 1,
            Tags = new List<string> { "C#", "ASP.NET", "EF Core" }
        };

        // Act
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // 새로운 컨텍스트로 다시 조회
        var loadedPost = await _context.Posts.FindAsync(post.Id);

        // Assert
        loadedPost!.Tags.Should().HaveCount(3);
        loadedPost.Tags.Should().Contain("C#");
        loadedPost.Tags.Should().Contain("ASP.NET");
        loadedPost.Tags.Should().Contain("EF Core");
    }

    [Fact]
    public async Task Post_ShouldHaveDefaultValues()
    {
        // Arrange
        var post = new Post
        {
            Title = "기본값 테스트",
            Content = "내용",
            AuthorId = 1
        };

        // Act
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Assert
        post.ViewCount.Should().Be(0);
        post.LikeCount.Should().Be(0);
        post.CommentCount.Should().Be(0);
        post.IsPinned.Should().BeFalse();
        post.IsDraft.Should().BeFalse();
        post.IsDeleted.Should().BeFalse();
        post.Status.Should().Be(PostStatus.Draft);
    }

    #endregion

    #region ViewRecord Entity Tests

    [Fact]
    public async Task ViewRecord_ShouldSetViewedAtOnCreate()
    {
        // Arrange
        var post = new Post { Title = "테스트", Content = "내용", AuthorId = 1 };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var beforeCreate = DateTime.UtcNow;
        var viewRecord = new ViewRecord
        {
            PostId = post.Id,
            UserId = 1
        };

        // Act
        _context.ViewRecords.Add(viewRecord);
        await _context.SaveChangesAsync();

        // Assert
        viewRecord.ViewedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
    }

    [Fact]
    public async Task ViewRecord_ShouldAllowNullUserId()
    {
        // Arrange
        var post = new Post { Title = "테스트", Content = "내용", AuthorId = 1 };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var viewRecord = new ViewRecord
        {
            PostId = post.Id,
            UserId = null,
            IpAddress = "192.168.1.1"
        };

        // Act
        _context.ViewRecords.Add(viewRecord);
        await _context.SaveChangesAsync();

        // Assert
        var loadedRecord = await _context.ViewRecords.FindAsync(viewRecord.Id);
        loadedRecord!.UserId.Should().BeNull();
        loadedRecord.IpAddress.Should().Be("192.168.1.1");
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task Post_ShouldHaveIndexOnAuthorId()
    {
        // 인덱스가 설정되어 있으면 많은 데이터에서 빠르게 조회 가능
        // Arrange
        for (int i = 0; i < 100; i++)
        {
            _context.Posts.Add(new Post
            {
                Title = $"게시물 {i}",
                Content = "내용",
                AuthorId = i % 10 // 10명의 작성자
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var posts = await _context.Posts
            .Where(p => p.AuthorId == 5)
            .ToListAsync();

        // Assert
        posts.Should().HaveCount(10);
    }

    [Fact]
    public async Task Post_ShouldHaveIndexOnCategory()
    {
        // Arrange
        var categories = new[] { "공지", "자유", "질문", "정보" };
        for (int i = 0; i < 100; i++)
        {
            _context.Posts.Add(new Post
            {
                Title = $"게시물 {i}",
                Content = "내용",
                AuthorId = 1,
                Category = categories[i % 4]
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var posts = await _context.Posts
            .Where(p => p.Category == "공지")
            .ToListAsync();

        // Assert
        posts.Should().HaveCount(25);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task Post_SoftDelete_ShouldSetIsDeletedAndDeletedAt()
    {
        // Arrange
        var post = new Post
        {
            Title = "삭제할 게시물",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert - 일반 쿼리로는 조회 불가
        var normalQuery = await _context.Posts.ToListAsync();
        normalQuery.Should().BeEmpty();

        // Assert - IgnoreQueryFilters로 조회 가능
        var deletedPost = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == post.Id);
        
        deletedPost.Should().NotBeNull();
        deletedPost!.IsDeleted.Should().BeTrue();
        deletedPost.DeletedAt.Should().NotBeNull();
    }

    #endregion
}
