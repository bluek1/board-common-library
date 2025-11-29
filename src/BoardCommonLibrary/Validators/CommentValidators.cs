using BoardCommonLibrary.DTOs;
using FluentValidation;

namespace BoardCommonLibrary.Validators;

/// <summary>
/// 댓글 생성 요청 검증기
/// </summary>
public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("댓글 내용은 필수입니다.")
            .MaximumLength(2000).WithMessage("댓글은 2000자 이내여야 합니다.");
    }
}

/// <summary>
/// 댓글 수정 요청 검증기
/// </summary>
public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("댓글 내용은 필수입니다.")
            .MaximumLength(2000).WithMessage("댓글은 2000자 이내여야 합니다.");
    }
}
