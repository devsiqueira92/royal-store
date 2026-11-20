namespace RoyalStore.Domain.Labels;

/// <summary>
/// The editable part of a <see cref="LabelFormat"/>: its name, paper type and measurements, all in
/// millimetres. Input to <see cref="LabelTemplate.Create"/> and <see cref="LabelTemplate.Update"/>.
/// </summary>
public sealed record LabelFormatSpec(
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
    decimal MarginLeft);
