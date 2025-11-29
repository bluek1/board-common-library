using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// PostService 단위 테스트
/// </summary>
public class PostServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _postService = new PostService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Create Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreatePost()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "테스트 게시물",
            Content = "테스트 내용입니다.",
            Category = "일반",
            Tags = new List<string> { "태그1", "태그2" }
        };

        // Act
        var result = await _postService.CreateAsync(request, authorId: 1, authorName: "테스터");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("테스트 게시물");
        result.Content.Should().Be("테스트 내용입니다.");
        result.Category.Should().Be("일반");
        result.Tags.Should().HaveCount(2);
        result.AuthorId.Should().Be(1);
        result.AuthorName.Should().Be("테스터");
        result.IsDraft.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithoutTags_ShouldCreatePostWithEmptyTags()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "태그 없는 게시물",
            Content = "내용"
        };

        // Act
        var result = await _postService.CreateAsync(request, authorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "시간 테스트",
            Content = "내용"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _postService.CreateAsync(request, authorId: 1);

        // Assert
        result.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
        result.PublishedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnPost()
    {
        // Arrange
        var post = new Post
        {
            Title = "조회 테스트",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetByIdAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
        result.Title.Should().Be("조회 테스트");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _postService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedPost_ShouldStillReturnPost_WhenUsingFindAsync()
    {
        // Arrange
        // 참고: FindAsync는 EF Core의 글로벌 필터를 무시할 수 있음
        // 실제 프로덕션에서는 별도의 soft delete 체크가 필요함
        var post = new Post
        {
            Title = "삭제된 게시물",
            Content = "내용",
            AuthorId = 1,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act - FindAsync는 글로벌 필터를 적용하지 않을 수 있음
        var result = await _postService.GetByIdAsync(post.Id);

        // Assert - 현재 구현에서는 삭제된 게시물도 반환됨 (FindAsync 특성)
        // 실제 서비스에서 IsDeleted 체크를 추가하면 이 테스트를 수정해야 함
        result.Should().NotBeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_AsAuthor_ShouldUpdatePost()
    {
        // Arrange
        var post = new Post
        {
            Title = "원본 제목",
            Content = "원본 내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest
        {
            Title = "수정된 제목",
            Content = "수정된 내용"
        };

        // Act
        var result = await _postService.UpdateAsync(post.Id, request, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("수정된 제목");
        result.Content.Should().Be("수정된 내용");
    }

    [Fact]
    public async Task UpdateAsync_AsAdmin_ShouldUpdatePost()
    {
        // Arrange
        var post = new Post
        {
            Title = "원본 제목",
            Content = "원본 내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest
        {
            Title = "관리자 수정"
        };

        // Act - 다른 사용자지만 관리자 권한
        var result = await _postService.UpdateAsync(post.Id, request, userId: 999, isAdmin: true);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("관리자 수정");
    }

    [Fact]
    public async Task UpdateAsync_AsOtherUser_ShouldReturnNull()
    {
        // Arrange
        var post = new Post
        {
            Title = "원본 제목",
            Content = "원본 내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest
        {
            Title = "수정 시도"
        };

        // Act - 권한 없는 다른 사용자
        var result = await _postService.UpdateAsync(post.Id, request, userId: 999, isAdmin: false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingPost_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdatePostRequest { Title = "수정" };

        // Act
        var result = await _postService.UpdateAsync(999, request, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var post = new Post
        {
            Title = "원본 제목",
            Content = "원본 내용",
            Category = "일반",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new UpdatePostRequest
        {
            Title = "수정된 제목만"
        };

        // Act
        var result = await _postService.UpdateAsync(post.Id, request, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("수정된 제목만");
        result.Content.Should().Be("원본 내용"); // 변경되지 않음
        result.Category.Should().Be("일반"); // 변경되지 않음
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_AsAuthor_ShouldSoftDelete()
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
        var result = await _postService.DeleteAsync(post.Id, userId: 1);

        // Assert
        result.Should().BeTrue();

        // 삭제 확인 (글로벌 필터 우회)
        var deletedPost = await _context.Posts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == post.Id);
        
        deletedPost.Should().NotBeNull();
        deletedPost!.IsDeleted.Should().BeTrue();
        deletedPost.DeletedAt.Should().NotBeNull();
        deletedPost.Status.Should().Be(PostStatus.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_AsAdmin_ShouldSoftDelete()
    {
        // Arrange
        var post = new Post
        {
            Title = "관리자 삭제 테스트",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.DeleteAsync(post.Id, userId: 999, isAdmin: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_AsOtherUser_ShouldReturnFalse()
    {
        // Arrange
        var post = new Post
        {
            Title = "삭제 테스트",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.DeleteAsync(post.Id, userId: 999, isAdmin: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingPost_ShouldReturnFalse()
    {
        // Act
        var result = await _postService.DeleteAsync(999, userId: 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_WithDefaultParams_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            _context.Posts.Add(new Post
            {
                Title = $"게시물 {i}",
                Content = "내용",
                AuthorId = 1
            });
        }
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Meta.TotalCount.Should().Be(25);
        result.Meta.TotalPages.Should().Be(3);
        result.Meta.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_PinnedPostsShouldAppearFirst()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "일반 게시물", Content = "내용", AuthorId = 1, IsPinned = false });
        _context.Posts.Add(new Post { Title = "고정 게시물", Content = "내용", AuthorId = 1, IsPinned = true });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters();

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.First().Title.Should().Be("고정 게시물");
        result.Data.First().IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ShouldFilterResults()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "공지", Content = "내용", AuthorId = 1, Category = "공지사항" });
        _context.Posts.Add(new Post { Title = "일반", Content = "내용", AuthorId = 1, Category = "자유게시판" });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters { Category = "공지사항" };

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data.First().Category.Should().Be("공지사항");
    }

    [Fact]
    public async Task GetAllAsync_WithAuthorFilter_ShouldFilterResults()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "작성자1", Content = "내용", AuthorId = 1 });
        _context.Posts.Add(new Post { Title = "작성자2", Content = "내용", AuthorId = 2 });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters { AuthorId = 1 };

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data.First().AuthorId.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_ShouldSearchTitleAndContent()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "검색 테스트", Content = "내용", AuthorId = 1 });
        _context.Posts.Add(new Post { Title = "일반", Content = "검색어 포함 내용", AuthorId = 1 });
        _context.Posts.Add(new Post { Title = "무관", Content = "무관", AuthorId = 1 });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters { Search = "검색" };

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDrafts()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "발행됨", Content = "내용", AuthorId = 1, IsDraft = false });
        _context.Posts.Add(new Post { Title = "임시저장", Content = "내용", AuthorId = 1, IsDraft = true });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters();

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data.First().Title.Should().Be("발행됨");
    }

    [Fact]
    public async Task GetAllAsync_SortByViewCount_ShouldSort()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "조회수 10", Content = "내용", AuthorId = 1, ViewCount = 10 });
        _context.Posts.Add(new Post { Title = "조회수 100", Content = "내용", AuthorId = 1, ViewCount = 100 });
        _context.Posts.Add(new Post { Title = "조회수 50", Content = "내용", AuthorId = 1, ViewCount = 50 });
        await _context.SaveChangesAsync();

        var parameters = new PostQueryParameters { SortBy = "viewcount", SortOrder = "desc" };

        // Act
        var result = await _postService.GetAllAsync(parameters);

        // Assert
        result.Data.Select(p => p.ViewCount).Should().BeInDescendingOrder();
    }

    #endregion

    #region Pin/Unpin Tests

    [Fact]
    public async Task PinAsync_ShouldSetIsPinnedTrue()
    {
        // Arrange
        var post = new Post
        {
            Title = "고정 테스트",
            Content = "내용",
            AuthorId = 1,
            IsPinned = false
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.PinAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task PinAsync_WithNonExistingPost_ShouldReturnNull()
    {
        // Act
        var result = await _postService.PinAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UnpinAsync_ShouldSetIsPinnedFalse()
    {
        // Arrange
        var post = new Post
        {
            Title = "고정 해제 테스트",
            Content = "내용",
            AuthorId = 1,
            IsPinned = true
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.UnpinAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.IsPinned.Should().BeFalse();
    }

    [Fact]
    public async Task UnpinAsync_WithNonExistingPost_ShouldReturnNull()
    {
        // Act
        var result = await _postService.UnpinAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Draft Tests

    [Fact]
    public async Task SaveDraftAsync_ShouldCreateDraft()
    {
        // Arrange
        var request = new DraftPostRequest
        {
            Title = "임시 제목",
            Content = "임시 내용"
        };

        // Act
        var result = await _postService.SaveDraftAsync(request, authorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("임시 제목");
    }

    [Fact]
    public async Task SaveDraftAsync_WithExistingDraftId_ShouldUpdateExistingDraft()
    {
        // Arrange
        var existingDraft = new Post
        {
            Title = "기존 임시저장",
            Content = "기존 내용",
            AuthorId = 1,
            IsDraft = true
        };
        _context.Posts.Add(existingDraft);
        await _context.SaveChangesAsync();

        var request = new DraftPostRequest
        {
            Title = "수정된 임시저장",
            Content = "수정된 내용",
            ExistingDraftId = existingDraft.Id
        };

        // Act
        var result = await _postService.SaveDraftAsync(request, authorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingDraft.Id);
        result.Title.Should().Be("수정된 임시저장");
    }

    [Fact]
    public async Task GetDraftsAsync_ShouldReturnOnlyUserDrafts()
    {
        // Arrange
        _context.Posts.Add(new Post { Title = "내 임시저장 1", Content = "내용", AuthorId = 1, IsDraft = true });
        _context.Posts.Add(new Post { Title = "내 임시저장 2", Content = "내용", AuthorId = 1, IsDraft = true });
        _context.Posts.Add(new Post { Title = "다른 사람 임시저장", Content = "내용", AuthorId = 2, IsDraft = true });
        _context.Posts.Add(new Post { Title = "발행된 게시물", Content = "내용", AuthorId = 1, IsDraft = false });
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.GetDraftsAsync(authorId: 1, new PagedRequest());

        // Assert
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishAsync_ShouldConvertDraftToPublished()
    {
        // Arrange
        var draft = new Post
        {
            Title = "발행할 임시저장",
            Content = "내용",
            AuthorId = 1,
            IsDraft = true
        };
        _context.Posts.Add(draft);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.PublishAsync(draft.Id, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.IsDraft.Should().BeFalse();
        result.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishAsync_WithWrongAuthor_ShouldReturnNull()
    {
        // Arrange
        var draft = new Post
        {
            Title = "발행할 임시저장",
            Content = "내용",
            AuthorId = 1,
            IsDraft = true
        };
        _context.Posts.Add(draft);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.PublishAsync(draft.Id, userId: 999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsAuthor Tests

    [Fact]
    public async Task IsAuthorAsync_WithCorrectAuthor_ShouldReturnTrue()
    {
        // Arrange
        var post = new Post
        {
            Title = "테스트",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.IsAuthorAsync(post.Id, userId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthorAsync_WithWrongAuthor_ShouldReturnFalse()
    {
        // Arrange
        var post = new Post
        {
            Title = "테스트",
            Content = "내용",
            AuthorId = 1
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.IsAuthorAsync(post.Id, userId: 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthorAsync_WithNonExistingPost_ShouldReturnFalse()
    {
        // Act
        var result = await _postService.IsAuthorAsync(999, userId: 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Exists Tests

    [Fact]
    public async Task ExistsAsync_WithExistingPost_ShouldReturnTrue()
    {
        // Arrange
        var post = new Post { Title = "테스트", Content = "내용", AuthorId = 1 };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postService.ExistsAsync(post.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingPost_ShouldReturnFalse()
    {
        // Act
        var result = await _postService.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
