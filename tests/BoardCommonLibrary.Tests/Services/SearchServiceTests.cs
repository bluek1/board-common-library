using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// SearchService 단위 테스트
/// </summary>
public class SearchServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly Mock<ILogger<SearchService>> _mockLogger;
    private readonly SearchService _service;
    
    public SearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BoardDbContext(options);
        _mockLogger = new Mock<ILogger<SearchService>>();
        _service = new SearchService(_context, _mockLogger.Object);
        
        SeedTestData();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    private void SeedTestData()
    {
        var posts = new List<Post>
        {
            new Post
            {
                Title = "테스트 게시물 1",
                Content = "첫 번째 테스트 게시물 내용입니다.",
                AuthorId = 1,
                AuthorName = "작성자1",
                Status = PostStatus.Published,
                Category = "공지사항",
                Tags = new List<string> { "테스트", "공지" },
                ViewCount = 100,
                LikeCount = 10,
                CommentCount = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Post
            {
                Title = "테스트 게시물 2",
                Content = "두 번째 테스트 게시물 내용입니다. 검색 키워드 포함.",
                AuthorId = 1,
                AuthorName = "작성자1",
                Status = PostStatus.Published,
                Category = "자유게시판",
                Tags = new List<string> { "테스트", "자유" },
                ViewCount = 50,
                LikeCount = 5,
                CommentCount = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Post
            {
                Title = "다른 제목의 게시물",
                Content = "특별한 내용이 있는 게시물입니다.",
                AuthorId = 2,
                AuthorName = "작성자2",
                Status = PostStatus.Published,
                Category = "공지사항",
                Tags = new List<string> { "특별", "중요" },
                ViewCount = 200,
                LikeCount = 20,
                CommentCount = 10,
                IsPinned = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Post
            {
                Title = "삭제된 게시물",
                Content = "이 게시물은 삭제되었습니다.",
                AuthorId = 1,
                AuthorName = "작성자1",
                Status = PostStatus.Deleted,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            }
        };
        
        _context.Posts.AddRange(posts);
        _context.SaveChanges();
    }
    
    #region SearchPostsAsync Tests
    
    [Fact]
    public async Task SearchPostsAsync_NoQuery_ReturnsAllPublishedPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithTitleQuery_ReturnsMatchingPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Query = "테스트",
            SearchIn = "title",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Contains("테스트", p.Title));
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithContentQuery_ReturnsMatchingPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Query = "키워드",
            SearchIn = "content",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Single(result.Items);
        Assert.Contains("키워드", result.Items.First().ContentPreview);
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithCategoryFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Category = "공지사항",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Equal("공지사항", p.Category));
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithTagFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Tags = "테스트",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Contains("테스트", p.Tags));
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithAuthorIdFilter_ReturnsFilteredPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            AuthorId = 1,
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Equal(1, p.AuthorId));
    }
    
    [Fact]
    public async Task SearchPostsAsync_SortByViewCount_ReturnsSortedResults()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Sort = "viewcount",
            Order = "desc",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        var viewCounts = result.Items.Select(p => p.ViewCount).ToList();
        Assert.Equal(viewCounts.OrderByDescending(v => v), viewCounts);
    }
    
    [Fact]
    public async Task SearchPostsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Page = 1,
            PageSize = 2
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }
    
    [Fact]
    public async Task SearchPostsAsync_IncludesPinnedFirst_WhenEnabled()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            IncludePinned = true,
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.True(result.Items.First().IsPinned);
    }
    
    [Fact]
    public async Task SearchPostsAsync_HighlightsSearchTerms()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Query = "테스트",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.All(result.Items, p => Assert.Contains("<mark>", p.TitleHighlighted));
    }
    
    [Fact]
    public async Task SearchPostsAsync_DoesNotReturnDeletedPosts()
    {
        // Arrange
        var parameters = new PostSearchParameters
        {
            Query = "삭제",
            Page = 1,
            PageSize = 20
        };
        
        // Act
        var result = await _service.SearchPostsAsync(parameters);
        
        // Assert
        Assert.Empty(result.Items);
    }
    
    #endregion
    
    #region SearchTagsAsync Tests
    
    [Fact]
    public async Task SearchTagsAsync_ReturnsMatchingTags()
    {
        // Arrange
        var parameters = new TagSearchParameters
        {
            Query = "테스트",
            Limit = 10
        };
        
        // Act
        var result = await _service.SearchTagsAsync(parameters);
        
        // Assert
        Assert.Single(result);
        Assert.Equal("테스트", result.First().Name);
        Assert.Equal(2, result.First().UsageCount);
    }
    
    [Fact]
    public async Task SearchTagsAsync_ReturnsHighlightedNames()
    {
        // Arrange
        var parameters = new TagSearchParameters
        {
            Query = "테스트",
            Limit = 10
        };
        
        // Act
        var result = await _service.SearchTagsAsync(parameters);
        
        // Assert
        Assert.Contains("<mark>", result.First().NameHighlighted);
    }
    
    [Fact]
    public async Task SearchTagsAsync_RespectsLimit()
    {
        // Arrange
        var parameters = new TagSearchParameters
        {
            Query = "테",
            Limit = 1
        };
        
        // Act
        var result = await _service.SearchTagsAsync(parameters);
        
        // Assert
        Assert.Single(result);
    }
    
    #endregion
    
    #region SearchAuthorsAsync Tests
    
    [Fact]
    public async Task SearchAuthorsAsync_ReturnsMatchingAuthors()
    {
        // Arrange
        var parameters = new AuthorSearchParameters
        {
            Query = "작성자",
            Limit = 10
        };
        
        // Act
        var result = await _service.SearchAuthorsAsync(parameters);
        
        // Assert
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task SearchAuthorsAsync_ReturnsPostCount()
    {
        // Arrange
        var parameters = new AuthorSearchParameters
        {
            Query = "작성자1",
            Limit = 10
        };
        
        // Act
        var result = await _service.SearchAuthorsAsync(parameters);
        
        // Assert
        var author1 = result.FirstOrDefault(a => a.Name == "작성자1");
        Assert.NotNull(author1);
        Assert.Equal(2, author1.PostCount);
    }
    
    #endregion
    
    #region SearchAllAsync Tests
    
    [Fact]
    public async Task SearchAllAsync_ReturnsAllCategories()
    {
        // Arrange
        var parameters = new UnifiedSearchParameters
        {
            Query = "테스트",
            SearchType = "all",
            Page = 1,
            PageSize = 10
        };
        
        // Act
        var result = await _service.SearchAllAsync(parameters);
        
        // Assert
        Assert.NotNull(result.Posts);
        Assert.NotNull(result.Tags);
        Assert.NotNull(result.Authors);
        Assert.Equal("테스트", result.Query);
    }
    
    [Fact]
    public async Task SearchAllAsync_OnlyPosts_ReturnsOnlyPosts()
    {
        // Arrange
        var parameters = new UnifiedSearchParameters
        {
            Query = "테스트",
            SearchType = "posts",
            Page = 1,
            PageSize = 10
        };
        
        // Act
        var result = await _service.SearchAllAsync(parameters);
        
        // Assert
        Assert.NotEmpty(result.Posts.Items);
        Assert.Empty(result.Tags.Items);
        Assert.Empty(result.Authors.Items);
    }
    
    #endregion
    
    #region GetSuggestionsAsync Tests
    
    [Fact]
    public async Task GetSuggestionsAsync_ReturnsTitleSuggestions()
    {
        // Arrange
        var query = "테스트";
        
        // Act
        var result = await _service.GetSuggestionsAsync(query, 5);
        
        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Type == "post");
    }
    
    [Fact]
    public async Task GetSuggestionsAsync_ReturnsTagSuggestions()
    {
        // Arrange
        var query = "테스트";
        
        // Act
        var result = await _service.GetSuggestionsAsync(query, 10);
        
        // Assert
        Assert.Contains(result, s => s.Type == "tag");
    }
    
    [Fact]
    public async Task GetSuggestionsAsync_ShortQuery_ReturnsEmpty()
    {
        // Arrange
        var query = "테";
        
        // Act
        var result = await _service.GetSuggestionsAsync(query, 5);
        
        // Assert
        Assert.Empty(result);
    }
    
    #endregion
    
    #region HighlightSearchTerms Tests
    
    [Fact]
    public void HighlightSearchTerms_SingleTerm_Highlights()
    {
        // Arrange
        var text = "테스트 게시물입니다.";
        var query = "테스트";
        
        // Act
        var result = _service.HighlightSearchTerms(text, query);
        
        // Assert
        Assert.Contains("<mark>테스트</mark>", result);
    }
    
    [Fact]
    public void HighlightSearchTerms_MultipleTerms_HighlightsAll()
    {
        // Arrange
        var text = "테스트 게시물입니다.";
        var query = "테스트 게시물";
        
        // Act
        var result = _service.HighlightSearchTerms(text, query);
        
        // Assert
        Assert.Contains("<mark>테스트</mark>", result);
        Assert.Contains("<mark>게시물</mark>", result);
    }
    
    [Fact]
    public void HighlightSearchTerms_CustomTag_UsesCustomTag()
    {
        // Arrange
        var text = "테스트 게시물입니다.";
        var query = "테스트";
        
        // Act
        var result = _service.HighlightSearchTerms(text, query, "em");
        
        // Assert
        Assert.Contains("<em>테스트</em>", result);
    }
    
    [Fact]
    public void HighlightSearchTerms_EmptyQuery_ReturnsOriginal()
    {
        // Arrange
        var text = "테스트 게시물입니다.";
        var query = "";
        
        // Act
        var result = _service.HighlightSearchTerms(text, query);
        
        // Assert
        Assert.Equal(text, result);
    }
    
    #endregion
    
    #region GenerateContentPreview Tests
    
    [Fact]
    public void GenerateContentPreview_ShortContent_ReturnsWhole()
    {
        // Arrange
        var content = "짧은 내용입니다.";
        
        // Act
        var result = _service.GenerateContentPreview(content, "");
        
        // Assert
        Assert.Equal(content, result);
    }
    
    [Fact]
    public void GenerateContentPreview_LongContent_Truncates()
    {
        // Arrange
        var content = new string('가', 300);
        
        // Act
        var result = _service.GenerateContentPreview(content, "", 100);
        
        // Assert
        Assert.True(result.Length <= 103); // 100 + "..."
        Assert.EndsWith("...", result);
    }
    
    [Fact]
    public void GenerateContentPreview_WithQuery_CentersOnQuery()
    {
        // Arrange
        var content = new string('가', 100) + "검색어" + new string('나', 100);
        
        // Act
        var result = _service.GenerateContentPreview(content, "검색어", 50);
        
        // Assert
        Assert.Contains("검색어", result);
    }
    
    [Fact]
    public void GenerateContentPreview_HtmlContent_RemovesTags()
    {
        // Arrange
        var content = "<p>테스트 <strong>내용</strong>입니다.</p>";
        
        // Act
        var result = _service.GenerateContentPreview(content, "");
        
        // Assert
        Assert.DoesNotContain("<p>", result);
        Assert.DoesNotContain("<strong>", result);
        Assert.Contains("테스트", result);
        Assert.Contains("내용", result);
    }
    
    [Fact]
    public void GenerateContentPreview_EmptyContent_ReturnsEmpty()
    {
        // Act
        var result = _service.GenerateContentPreview("", "query");
        
        // Assert
        Assert.Empty(result);
    }
    
    #endregion
}
