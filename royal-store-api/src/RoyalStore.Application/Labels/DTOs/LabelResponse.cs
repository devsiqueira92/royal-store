using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Labels.DTOs;

/// <summary>
/// A label as the designer sends it — <c>{ format, template }</c> — plus identifiers and times
/// (065 contracts/labels-api.md). <see cref="Format"/> matches the UI's <c>LabelFormat</c>,
/// <see cref="Template"/> its <c>LabelTemplate</c>.
/// </summary>
public sealed record LabelResponse(LabelFormatResponse Format, LabelTemplateResponse Template)
{
    public static LabelResponse From(LabelTemplate label) =>
        new(LabelFormatResponse.From(label.Format), LabelTemplateResponse.From(label));
}

public sealed record LabelFormatResponse(
    Guid Id,
    string Name,
    LabelPaperType PaperType,
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
    decimal MarginLeft,
    bool IsSystem,
    bool IsActive)
{
    public static LabelFormatResponse From(LabelFormat format) =>
        new(format.Id, format.Name, format.PaperType,
            format.PaperWidth, format.PaperHeight, format.LabelWidth, format.LabelHeight,
            format.Columns, format.HorizontalSpacing, format.VerticalSpacing,
            format.MarginTop, format.MarginRight, format.MarginBottom, format.MarginLeft,
            format.IsSystem, format.IsActive);
}

public sealed record LabelTemplateResponse(
    Guid Id,
    string Name,
    Guid LabelFormatId,
    string? SourceTemplateId,
    string Configuration,
    bool IsDefault,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static LabelTemplateResponse From(LabelTemplate label) =>
        new(label.Id, label.Name, label.LabelFormatId, label.SourceTemplateId, label.Configuration,
            label.IsDefault, label.IsActive, label.CreatedAt,
            // A change to the measurements alone updates only the format row (065 research D11).
            label.Format.UpdatedAt > label.UpdatedAt ? label.Format.UpdatedAt : label.UpdatedAt);
}
