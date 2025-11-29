using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Validators;
using FluentValidation.TestHelper;

namespace BoardCommonLibrary.Tests.Validators;

/// <summary>
/// Comment Validator 단위 테스트
/// </summary>
public class CommentValidatorsTests
{
    private readonly CreateCommentRequestValidator _createValidator;
    private readonly UpdateCommentRequestValidator _updateValidator;
    
    public CommentValidatorsTests()
    {
        _createValidator = new CreateCommentRequestValidator();
        _updateValidator = new UpdateCommentRequestValidator();
    }
    
    #region CreateCommentRequestValidator Tests
    
    [Fact]
    public void CreateCommentValidator_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = "테스트 댓글 내용입니다."
        };
        
        // Act
        var result = _createValidator.TestValidate(request);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCommentValidator_EmptyContent_ShouldHaveError(string? content)
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = content!
        };
        
        // Act
        var result = _createValidator.TestValidate(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }
    
    [Fact]
    public void CreateCommentValidator_ContentTooLong_ShouldHaveError()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = new string('가', 2001) // 2000자 초과
        };
        
        // Act
        var result = _createValidator.TestValidate(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("댓글은 2000자 이내여야 합니다.");
    }
    
    [Fact]
    public void CreateCommentValidator_ContentMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateCommentRequest
        {
            Content = new string('가', 2000) // 정확히 2000자
        };
        
        // Act
        var result = _createValidator.TestValidate(request);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }
    
    #endregion
    
    #region UpdateCommentRequestValidator Tests
    
    [Fact]
    public void UpdateCommentValidator_ValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new UpdateCommentRequest
        {
            Content = "수정된 댓글 내용입니다."
        };
        
        // Act
        var result = _updateValidator.TestValidate(request);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCommentValidator_EmptyContent_ShouldHaveError(string? content)
    {
        // Arrange
        var request = new UpdateCommentRequest
        {
            Content = content!
        };
        
        // Act
        var result = _updateValidator.TestValidate(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }
    
    [Fact]
    public void UpdateCommentValidator_ContentTooLong_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateCommentRequest
        {
            Content = new string('나', 2001) // 2000자 초과
        };
        
        // Act
        var result = _updateValidator.TestValidate(request);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("댓글은 2000자 이내여야 합니다.");
    }
    
    [Fact]
    public void UpdateCommentValidator_ContentMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateCommentRequest
        {
            Content = new string('나', 2000) // 정확히 2000자
        };
        
        // Act
        var result = _updateValidator.TestValidate(request);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }
    
    #endregion
}
