using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BoardCommonLibrary.Tests.Validators;

/// <summary>
/// PostValidators 단위 테스트
/// </summary>
public class PostValidatorsTests
{
    #region CreatePostRequestValidator Tests

    private readonly CreatePostRequestValidator _createValidator = new();

    [Fact]
    public void CreatePostRequest_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "유효한 제목",
            Content = "유효한 내용입니다."
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePostRequest_WithEmptyTitle_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "",
            Content = "내용"
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePostRequest_WithNullTitle_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = null!,
            Content = "내용"
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePostRequest_WithTitleExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = new string('가', 201), // 201자
            Content = "내용"
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("제목은 200자 이내여야 합니다.");
    }

    [Fact]
    public void CreatePostRequest_WithEmptyContent_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "제목",
            Content = ""
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreatePostRequest_WithNullContent_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "제목",
            Content = null!
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreatePostRequest_WithWhitespaceOnlyTitle_ShouldFailValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "   ",
            Content = "내용"
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePostRequest_WithMaxLengthTitle_ShouldPassValidation()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = new string('가', 200), // 정확히 200자
            Content = "내용"
        };

        // Act
        var result = _createValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region UpdatePostRequestValidator Tests

    private readonly UpdatePostRequestValidator _updateValidator = new();

    [Fact]
    public void UpdatePostRequest_WithEmptyRequest_ShouldPassValidation()
    {
        // Arrange - 업데이트는 모든 필드가 선택적
        var request = new UpdatePostRequest();

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePostRequest_WithValidTitle_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdatePostRequest
        {
            Title = "수정된 제목"
        };

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePostRequest_WithTitleExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdatePostRequest
        {
            Title = new string('가', 201)
        };

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdatePostRequest_WithEmptyTitleString_ShouldPassValidation()
    {
        // Arrange - 빈 문자열도 허용 (업데이트 안 함으로 처리)
        var request = new UpdatePostRequest
        {
            Title = ""
        };

        // Act
        var result = _updateValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region DraftPostRequestValidator Tests

    private readonly DraftPostRequestValidator _draftValidator = new();

    [Fact]
    public void DraftPostRequest_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new DraftPostRequest
        {
            Title = "임시 제목",
            Content = "임시 내용"
        };

        // Act
        var result = _draftValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DraftPostRequest_WithEmptyTitle_ShouldPassValidation()
    {
        // Arrange - 임시저장은 제목이 비어있어도 허용됨
        var request = new DraftPostRequest
        {
            Title = "",
            Content = "내용"
        };

        // Act
        var result = _draftValidator.TestValidate(request);

        // Assert - 임시저장은 유효성 검증을 느슨하게 적용
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DraftPostRequest_WithTitleExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var request = new DraftPostRequest
        {
            Title = new string('가', 201),
            Content = "내용"
        };

        // Act
        var result = _draftValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void DraftPostRequest_WithExistingDraftId_ShouldPassValidation()
    {
        // Arrange
        var request = new DraftPostRequest
        {
            Title = "제목",
            Content = "내용",
            ExistingDraftId = 123
        };

        // Act
        var result = _draftValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
