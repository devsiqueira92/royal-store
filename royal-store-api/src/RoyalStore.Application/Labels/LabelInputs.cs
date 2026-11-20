using FluentValidation;
using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Labels;

/// <summary>
/// The <c>format</c> part of the designer's request (065). <see cref="IsActive"/> is accepted and
/// ignored — the template's flag decides the label's state (FR-008).
/// </summary>
public sealed record LabelFormatInput(
    Guid? Id,
    string Name,
    LabelPaperType PaperType,
    bool? IsActive,
    decimal PaperWidth,
    decimal PaperHeight,
    decimal LabelWidth,
    decimal LabelHeight,
    int Columns,
    decimal HorizontalSpacing,
    decimal VerticalSpacing,
    decimal MarginTop,
    decimal MarginRight,
    decimal MarginBottom,
    decimal MarginLeft)
{
    public LabelFormatSpec ToSpec() =>
        new(Name, PaperType, PaperWidth, PaperHeight, LabelWidth, LabelHeight, Columns,
            HorizontalSpacing, VerticalSpacing, MarginTop, MarginRight, MarginBottom, MarginLeft);
}

/// <summary>The <c>template</c> part of the designer's request (065).</summary>
public sealed record LabelTemplateInput(
    Guid? Id,
    string Name,
    Guid? LabelFormatId,
    string? SourceTemplateId,
    string Configuration,
    bool IsDefault,
    bool IsActive);

/// <summary>
/// Request-shape rules shared by the create and update validators. They ask the domain for every
/// rule the request breaks — not just the first — so one refusal lists them all (FR-018); the
/// aggregate re-checks the same rules as the guarantee.
/// </summary>
public static class LabelInputs
{
    public static void Validate<T>(
        LabelFormatInput? format, LabelTemplateInput? template, ValidationContext<T> context)
    {
        if (format is null)
            context.AddFailure("Format", "The label format is required.");
        else
            foreach (var error in LabelFormat.Check(format.ToSpec()))
                context.AddFailure("Format", error.Message);

        if (template is null)
            context.AddFailure("Template", "The label template is required.");
        else
            foreach (var error in LabelTemplate.Check(
                         template.Name, template.Configuration, template.SourceTemplateId,
                         template.IsDefault, template.IsActive))
                context.AddFailure("Template", error.Message);
    }
}
