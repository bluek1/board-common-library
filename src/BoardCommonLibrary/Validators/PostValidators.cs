using BoardCommonLibrary.DTOs;
using FluentValidation;

namespace BoardCommonLibrary.Validators;

/// <summary>
/// 게시물 생성 요청 검증기
/// </summary>
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("제목은 필수입니다.")
            .MaximumLength(200).WithMessage("제목은 200자 이내여야 합니다.");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("본문은 필수입니다.");
        
        RuleFor(x => x.Category)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("카테고리는 100자 이내여야 합니다.");
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}

/// <summary>
/// 게시물 수정 요청 검증기
/// </summary>
public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("제목은 200자 이내여야 합니다.");
        
        RuleFor(x => x.Category)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("카테고리는 100자 이내여야 합니다.");
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}

/// <summary>
/// 임시저장 요청 검증기
/// </summary>
public class DraftPostRequestValidator : AbstractValidator<DraftPostRequest>
{
    public DraftPostRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("제목은 200자 이내여야 합니다.");
        
        RuleFor(x => x.Category)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("카테고리는 100자 이내여야 합니다.");
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}
