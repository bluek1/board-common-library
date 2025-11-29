using BoardCommonLibrary.DTOs;
using FluentValidation;

namespace BoardCommonLibrary.Validators;

/// <summary>
/// Q&A 질문 작성 요청 검증기
/// </summary>
public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("제목은 필수입니다.")
            .MaximumLength(200).WithMessage("제목은 200자 이내여야 합니다.");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("내용은 필수입니다.");
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}

/// <summary>
/// Q&A 질문 수정 요청 검증기
/// </summary>
public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("제목은 필수입니다.")
            .MaximumLength(200).WithMessage("제목은 200자 이내여야 합니다.")
            .When(x => x.Title != null);
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("내용은 필수입니다.")
            .When(x => x.Content != null);
        
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10)
            .WithMessage("태그는 최대 10개까지 가능합니다.");
    }
}

/// <summary>
/// Q&A 답변 작성 요청 검증기
/// </summary>
public class CreateAnswerRequestValidator : AbstractValidator<CreateAnswerRequest>
{
    public CreateAnswerRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("답변 내용은 필수입니다.");
    }
}

/// <summary>
/// Q&A 답변 수정 요청 검증기
/// </summary>
public class UpdateAnswerRequestValidator : AbstractValidator<UpdateAnswerRequest>
{
    public UpdateAnswerRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("답변 내용은 필수입니다.")
            .When(x => x.Content != null);
    }
}

/// <summary>
/// 신고 작성 요청 검증기
/// </summary>
public class CreateReportRequestValidator : AbstractValidator<CreateReportRequest>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum().WithMessage("유효하지 않은 대상 유형입니다.");
        
        RuleFor(x => x.TargetId)
            .GreaterThan(0).WithMessage("유효하지 않은 대상 ID입니다.");
        
        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("유효하지 않은 신고 사유입니다.");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("상세 설명은 500자 이내여야 합니다.")
            .When(x => x.Description != null);
    }
}

/// <summary>
/// 신고 처리 요청 검증기 (관리자용)
/// </summary>
public class ProcessReportRequestValidator : AbstractValidator<ProcessReportRequest>
{
    public ProcessReportRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("유효하지 않은 처리 상태입니다.")
            .Must(status => status != Entities.ReportStatus.Pending)
            .WithMessage("대기 상태로 변경할 수 없습니다.");
        
        RuleFor(x => x.ProcessingNote)
            .MaximumLength(500).WithMessage("처리 메모는 500자 이내여야 합니다.")
            .When(x => x.ProcessingNote != null);
    }
}
