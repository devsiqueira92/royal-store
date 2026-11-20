using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Labels;

/// <summary>
/// The physical medium of a label (065): paper type, paper and label size, columns, spacing and
/// margins, in millimetres. Belongs to exactly one <see cref="LabelTemplate"/>, which creates,
/// changes, activates and removes it (FR-021).
/// </summary>
public sealed class LabelFormat : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;
    public const decimal MaxMeasurement = 1000m;
    public const int MinColumns = 1;
    public const int MaxColumns = 20;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public LabelPaperType PaperType { get; private set; }

    public decimal PaperWidth { get; private set; }
    public decimal PaperHeight { get; private set; }
    public decimal LabelWidth { get; private set; }
    public decimal LabelHeight { get; private set; }

    public int Columns { get; private set; }

    public decimal HorizontalSpacing { get; private set; }
    public decimal VerticalSpacing { get; private set; }

    public decimal MarginTop { get; private set; }
    public decimal MarginRight { get; private set; }
    public decimal MarginBottom { get; private set; }
    public decimal MarginLeft { get; private set; }

    /// <summary>Always false: the designer's pre-configurations are not stored (FR-007, FR-016).</summary>
    public bool IsSystem { get; private set; }

    /// <summary>Always equal to the owning template's <see cref="LabelTemplate.IsActive"/> (FR-008).</summary>
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private LabelFormat() : base(Guid.NewGuid()) { }

    private LabelFormat(Guid id, Guid tenantId, bool isActive) : base(id)
    {
        TenantId = tenantId;
        IsActive = isActive;
    }

    /// <summary>
    /// Every rule the spec breaks, in a fixed order (065 research D5). The fit rules are only checked
    /// once every field is valid on its own — a fit computed from a negative width means nothing.
    /// The request validator reports all of them at once (FR-018); the factory returns the first.
    /// </summary>
    public static IReadOnlyList<Error> Check(LabelFormatSpec spec)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(spec.Name))
            errors.Add(Error.Validation("labelformat.name_required", "Format name is required."));
        else if (spec.Name.Trim().Length > NameMaxLength)
            errors.Add(Error.Validation("labelformat.name_too_long",
                $"Format name must not exceed {NameMaxLength} characters."));

        if (!Enum.IsDefined(spec.PaperType))
            errors.Add(Error.Validation("labelformat.paper_type_invalid",
                "Paper type must be 0 (sheet) or 1 (roll)."));

        foreach (var (label, value) in Sizes(spec))
        {
            if (value <= 0 || value > MaxMeasurement)
                errors.Add(Error.Validation("labelformat.size_out_of_range",
                    $"{label} must be greater than 0 and at most {MaxMeasurement:0} mm."));
        }

        foreach (var (label, value) in Offsets(spec))
        {
            if (value < 0 || value > MaxMeasurement)
                errors.Add(Error.Validation("labelformat.spacing_out_of_range",
                    $"{label} must be between 0 and {MaxMeasurement:0} mm."));
        }

        foreach (var (label, value) in Sizes(spec).Concat(Offsets(spec)))
        {
            if (value != decimal.Round(value, 2))
                errors.Add(Error.Validation("labelformat.precision",
                    $"{label} must have at most two decimal places."));
        }

        if (spec.Columns < MinColumns || spec.Columns > MaxColumns)
            errors.Add(Error.Validation("labelformat.columns_out_of_range",
                $"Columns must be a whole number from {MinColumns} to {MaxColumns}."));

        if (errors.Count > 0)
            return errors;

        if (spec.LabelWidth > spec.PaperWidth || spec.LabelHeight > spec.PaperHeight)
            errors.Add(Error.Validation("labelformat.label_larger_than_paper",
                "Label width and height must not exceed the paper width and height."));

        var usedWidth = spec.MarginLeft + spec.Columns * spec.LabelWidth
                        + (spec.Columns - 1) * spec.HorizontalSpacing + spec.MarginRight;
        if (usedWidth > spec.PaperWidth)
            errors.Add(Error.Validation("labelformat.columns_do_not_fit",
                $"Margins, {spec.Columns} column(s) and spacing take {usedWidth} mm, more than the paper width of {spec.PaperWidth} mm."));

        var usedHeight = spec.MarginTop + spec.LabelHeight + spec.MarginBottom;
        if (usedHeight > spec.PaperHeight)
            errors.Add(Error.Validation("labelformat.height_does_not_fit",
                $"Top margin, label height and bottom margin take {usedHeight} mm, more than the paper height of {spec.PaperHeight} mm."));

        return errors;
    }

    public static Result Validate(LabelFormatSpec spec)
    {
        var errors = Check(spec);
        return errors.Count == 0 ? Result.Success() : Result.Failure(errors[0]);
    }

    internal static Result<LabelFormat> Create(Guid tenantId, LabelFormatSpec spec, bool isActive)
    {
        var valid = Validate(spec);
        if (valid.IsFailure)
            return valid.Error;

        var format = new LabelFormat(Guid.NewGuid(), tenantId, isActive);
        format.Apply(spec);
        return format;
    }

    internal Result Update(LabelFormatSpec spec)
    {
        var valid = Validate(spec);
        if (valid.IsFailure)
            return valid;

        Apply(spec);
        return Result.Success();
    }

    internal void SetActive(bool isActive) => IsActive = isActive;

    private void Apply(LabelFormatSpec spec)
    {
        Name = spec.Name.Trim();
        PaperType = spec.PaperType;
        PaperWidth = spec.PaperWidth;
        PaperHeight = spec.PaperHeight;
        LabelWidth = spec.LabelWidth;
        LabelHeight = spec.LabelHeight;
        Columns = spec.Columns;
        HorizontalSpacing = spec.HorizontalSpacing;
        VerticalSpacing = spec.VerticalSpacing;
        MarginTop = spec.MarginTop;
        MarginRight = spec.MarginRight;
        MarginBottom = spec.MarginBottom;
        MarginLeft = spec.MarginLeft;
    }

    private static IEnumerable<(string Label, decimal Value)> Sizes(LabelFormatSpec spec) =>
    [
        ("Paper width", spec.PaperWidth),
        ("Paper height", spec.PaperHeight),
        ("Label width", spec.LabelWidth),
        ("Label height", spec.LabelHeight),
    ];

    private static IEnumerable<(string Label, decimal Value)> Offsets(LabelFormatSpec spec) =>
    [
        ("Horizontal spacing", spec.HorizontalSpacing),
        ("Vertical spacing", spec.VerticalSpacing),
        ("Top margin", spec.MarginTop),
        ("Right margin", spec.MarginRight),
        ("Bottom margin", spec.MarginBottom),
        ("Left margin", spec.MarginLeft),
    ];
}
