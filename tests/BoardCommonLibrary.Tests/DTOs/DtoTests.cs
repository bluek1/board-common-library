using BoardCommonLibrary.DTOs;
using FluentAssertions;

namespace BoardCommonLibrary.Tests.DTOs;

/// <summary>
/// DTO 클래스 단위 테스트
/// </summary>
public class DtoTests
{
    #region PagedResponse Tests

    [Fact]
    public void PagedResponse_Create_ShouldCalculateTotalPages()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 1, pageSize: 10, totalCount: 25);

        // Assert
        result.Meta.TotalPages.Should().Be(3); // 25 / 10 = 2.5 → 3
    }

    [Fact]
    public void PagedResponse_Create_WithExactDivision_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 1, pageSize: 10, totalCount: 20);

        // Assert
        result.Meta.TotalPages.Should().Be(2); // 20 / 10 = 2
    }

    [Fact]
    public void PagedResponse_Create_WithZeroItems_ShouldReturnZeroPages()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = PagedResponse<string>.Create(items, page: 1, pageSize: 10, totalCount: 0);

        // Assert
        result.Meta.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public void PagedResponse_HasNextPage_ShouldBeTrue_WhenMorePagesExist()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 1, pageSize: 10, totalCount: 25);

        // Assert
        result.Meta.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResponse_HasNextPage_ShouldBeFalse_WhenOnLastPage()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 3, pageSize: 10, totalCount: 25);

        // Assert
        result.Meta.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_HasPreviousPage_ShouldBeFalse_WhenOnFirstPage()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 1, pageSize: 10, totalCount: 25);

        // Assert
        result.Meta.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_HasPreviousPage_ShouldBeTrue_WhenNotOnFirstPage()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResponse<string>.Create(items, page: 2, pageSize: 10, totalCount: 25);

        // Assert
        result.Meta.HasPreviousPage.Should().BeTrue();
    }

    #endregion

    #region ApiResponse Tests

    [Fact]
    public void ApiResponse_Ok_ShouldCreateSuccessResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = ApiResponse<object>.Ok(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void ApiErrorResponse_Create_ShouldCreateErrorResponse()
    {
        // Arrange & Act
        var result = ApiErrorResponse.Create("ERROR_001", "오류 발생");

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("ERROR_001");
        result.Error.Message.Should().Be("오류 발생");
    }

    [Fact]
    public void ApiErrorResponse_Create_WithDetails_ShouldIncludeDetails()
    {
        // Arrange
        var details = new List<ValidationError>
        {
            new() { Field = "Title", Message = "제목은 필수입니다." }
        };

        // Act
        var result = ApiErrorResponse.Create("VALIDATION_ERROR", "유효성 검증 실패", details);

        // Assert
        result.Error.Details.Should().NotBeNull();
        result.Error.Details.Should().HaveCount(1);
        result.Error.Details![0].Field.Should().Be("Title");
    }

    #endregion

    #region PostQueryParameters Tests

    [Fact]
    public void PostQueryParameters_ShouldHaveDefaultValues()
    {
        // Act
        var parameters = new PostQueryParameters();

        // Assert
        parameters.Page.Should().Be(1);
        parameters.PageSize.Should().Be(20);
        parameters.SortBy.Should().Be("createdAt");
        parameters.SortOrder.Should().Be("desc");
        parameters.Category.Should().BeNull();
        parameters.AuthorId.Should().BeNull();
        parameters.Search.Should().BeNull();
    }

    [Fact]
    public void PostQueryParameters_PageSize_ShouldBeCappedAt100()
    {
        // Arrange
        var parameters = new PostQueryParameters
        {
            PageSize = 150
        };

        // Assert - PageSize는 100으로 제한됨
        parameters.PageSize.Should().Be(100);
    }

    [Fact]
    public void PostQueryParameters_Page_ShouldNotBeLessThan1()
    {
        // Arrange
        var parameters = new PostQueryParameters
        {
            Page = 0
        };

        // Assert - Page는 1 이상으로 제한됨
        parameters.Page.Should().Be(1);
    }

    #endregion

    #region PostResponse Tests

    [Fact]
    public void PostResponse_ShouldInitializeWithEmptyTags()
    {
        // Act
        var response = new PostResponse();

        // Assert
        response.Tags.Should().NotBeNull();
        response.Tags.Should().BeEmpty();
    }

    [Fact]
    public void PostSummaryResponse_ShouldInitializeWithEmptyTags()
    {
        // Act
        var response = new PostSummaryResponse();

        // Assert
        response.Tags.Should().NotBeNull();
        response.Tags.Should().BeEmpty();
    }

    #endregion

    #region Request DTOs Tests

    [Fact]
    public void CreatePostRequest_ShouldAllowNullTags()
    {
        // Act
        var request = new CreatePostRequest
        {
            Title = "제목",
            Content = "내용"
        };

        // Assert
        request.Tags.Should().BeNull(); // null이면 서비스에서 빈 리스트로 처리
    }

    [Fact]
    public void UpdatePostRequest_AllFieldsShouldBeNullable()
    {
        // Act
        var request = new UpdatePostRequest();

        // Assert
        request.Title.Should().BeNull();
        request.Content.Should().BeNull();
        request.Category.Should().BeNull();
        request.Tags.Should().BeNull();
    }

    [Fact]
    public void DraftPostRequest_ShouldHaveOptionalExistingDraftId()
    {
        // Act
        var request = new DraftPostRequest
        {
            Title = "임시",
            Content = "내용"
        };

        // Assert
        request.ExistingDraftId.Should().BeNull();
    }

    [Fact]
    public void DraftPostResponse_ShouldHaveCorrectProperties()
    {
        // Act
        var response = new DraftPostResponse
        {
            Id = 1,
            Title = "임시 제목",
            ContentPreview = "미리보기...",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        response.Id.Should().Be(1);
        response.Title.Should().Be("임시 제목");
    }

    #endregion

    #region PagedMetaData Tests

    [Fact]
    public void PagedMetaData_HasNextPage_ShouldCalculateCorrectly()
    {
        // Arrange
        var meta = new PagedMetaData
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3
        };

        // Assert
        meta.HasNextPage.Should().BeTrue();
        meta.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedMetaData_OnLastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        var meta = new PagedMetaData
        {
            Page = 3,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3
        };

        // Assert
        meta.HasNextPage.Should().BeFalse();
        meta.HasPreviousPage.Should().BeTrue();
    }

    #endregion
}
