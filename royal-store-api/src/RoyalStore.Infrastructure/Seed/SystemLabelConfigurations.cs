using RoyalStore.Domain.Labels;

namespace RoyalStore.Infrastructure.Seed;

/// <summary>
/// The label configurations that come with the system (066 FR-009): the designer's three
/// pre-configurations, shared by every store and read-only. Seeded through EF model data, so adding or
/// changing one is a model change and therefore a new migration. Owned by no store —
/// <see cref="LabelTemplate.SystemTenantId"/>.
/// </summary>
public static class SystemLabelConfigurations
{
    public static readonly Guid A4FormatId = Guid.Parse("00000000-0000-0000-0066-0000000000f1");
    public static readonly Guid LetterFormatId = Guid.Parse("00000000-0000-0000-0066-0000000000f2");
    public static readonly Guid RollFormatId = Guid.Parse("00000000-0000-0000-0066-0000000000f3");

    public static readonly Guid A4TemplateId = Guid.Parse("00000000-0000-0000-0066-0000000000a1");
    public static readonly Guid LetterTemplateId = Guid.Parse("00000000-0000-0000-0066-0000000000a2");
    public static readonly Guid RollTemplateId = Guid.Parse("00000000-0000-0000-0066-0000000000a3");

    private static readonly DateTimeOffset SeededAt = new(2026, 11, 22, 0, 0, 0, TimeSpan.Zero);
    private const string SeededBy = "system";

    /// <summary>
    /// A4 sheet layout: name, SKU, price and barcode. <c>product.price</c> is the legacy price field the
    /// designer maps to each store's default price list, keeping system layouts store-agnostic.
    /// </summary>
    private const string A4Layout =
        """{"version":1,"elements":[{"type":"text","field":"product.name","x":1,"y":1.5,"width":61,"height":6.5,"fontSize":11,"fontWeight":"bold","alignment":"left"},{"type":"text","field":"product.price","x":1,"y":14.5,"width":60.5,"height":7.5,"fontSize":14,"fontWeight":"bold","alignment":"center","format":"currency"},{"type":"barcode","field":"product.barcode","x":2.5,"y":25.5,"width":59.5,"height":11,"format":"CODE128","showValue":true},{"type":"text","field":"product.sku","x":1,"y":8,"width":22,"height":5.5,"fontSize":6,"fontWeight":"normal","alignment":"left"}]}""";

    /// <summary>
    /// Six-column letter sheet layout: name, price and barcode. <c>product.price</c> keeps it store-agnostic.
    /// </summary>
    private const string LetterLayout =
        """{"version":1,"elements":[{"type":"text","field":"product.name","x":1,"y":0,"width":32,"height":5.5,"fontSize":6,"fontWeight":"bold","alignment":"left"},{"type":"text","field":"product.price","x":1,"y":6,"width":32,"height":4,"fontSize":8,"fontWeight":"bold","alignment":"center","format":"currency"},{"type":"barcode","field":"product.barcode","x":2,"y":10,"width":30,"height":7.5,"format":"CODE128","showValue":true}]}""";

    /// <summary>
    /// Three-column thermal roll layout: name, price and SKU barcode. <c>product.price</c> keeps it
    /// store-agnostic (mapped to each store's default price list by the designer).
    /// </summary>
    private const string RollLayout =
        """{"version":1,"elements":[{"type":"text","field":"product.name","x":1,"y":0,"width":30.5,"height":6.5,"fontSize":6,"fontWeight":"bold","alignment":"left"},{"type":"text","field":"product.price","x":1,"y":6.5,"width":16,"height":5.5,"fontSize":8,"fontWeight":"bold","alignment":"center","format":"currency","showLabel":false},{"type":"barcode","field":"product.sku","x":0.5,"y":14,"width":32,"height":6.5,"fontSize":4.5,"format":"CODE128","showValue":true}]}""";

    public static object[] Formats =>
    [
        Format(A4FormatId, "A4 — 3 × 7 (63,5 × 38,1 mm)", LabelPaperType.Sheet,
            paperWidth: 210m, paperHeight: 297m, labelWidth: 63.5m, labelHeight: 38.1m, columns: 3,
            horizontalSpacing: 2.5m, verticalSpacing: 0m, marginTop: 15.15m, marginRight: 7.25m, marginBottom: 15.15m, marginLeft: 7.25m),
        Format(LetterFormatId, "Carta — 6 colunas (33 × 18 mm)", LabelPaperType.Sheet,
            paperWidth: 216m, paperHeight: 279m, labelWidth: 33m, labelHeight: 18m, columns: 6,
            horizontalSpacing: 2m, verticalSpacing: 0m, marginTop: 12.7m, marginRight: 4m, marginBottom: 12.7m, marginLeft: 4m),
        Format(RollFormatId, "Rolo térmico 3 colunas (33 × 21,3 mm)", LabelPaperType.Roll,
            paperWidth: 107m, paperHeight: 300m, labelWidth: 33m, labelHeight: 21.3m, columns: 3,
            horizontalSpacing: 2m, verticalSpacing: 2m, marginTop: 2m, marginRight: 2m, marginBottom: 2m, marginLeft: 2m),
    ];

    public static object[] Templates =>
    [
        Template(A4TemplateId, "Gôndola A4 (3 colunas)", A4FormatId, A4Layout),
        Template(LetterTemplateId, "Gôndola Carta (6 colunas)", LetterFormatId, LetterLayout),
        Template(RollTemplateId, "Rolo térmico 3 colunas (33 × 21,3)", RollFormatId, RollLayout),
    ];

    private static object Format(
        Guid id, string name, LabelPaperType paperType,
        decimal paperWidth, decimal paperHeight, decimal labelWidth, decimal labelHeight, int columns,
        decimal horizontalSpacing, decimal verticalSpacing, decimal marginTop, decimal marginRight, decimal marginBottom, decimal marginLeft) =>
        new
        {
            Id = id,
            TenantId = LabelTemplate.SystemTenantId,
            Name = name,
            PaperType = paperType,
            PaperWidth = paperWidth,
            PaperHeight = paperHeight,
            LabelWidth = labelWidth,
            LabelHeight = labelHeight,
            Columns = columns,
            HorizontalSpacing = horizontalSpacing,
            VerticalSpacing = verticalSpacing,
            MarginTop = marginTop,
            MarginRight = marginRight,
            MarginBottom = marginBottom,
            MarginLeft = marginLeft,
            IsSystem = true,
            IsActive = true,
            CreatedAt = SeededAt,
            CreatedBy = SeededBy,
            UpdatedAt = SeededAt,
            UpdatedBy = SeededBy,
        };

    private static object Template(Guid id, string name, Guid formatId, string configuration) =>
        new
        {
            Id = id,
            TenantId = LabelTemplate.SystemTenantId,
            Name = name,
            NormalizedName = LabelTemplate.Normalize(name),
            LabelFormatId = formatId,
            Configuration = configuration,
            SourceTemplateId = (string?)null,
            IsDefault = false,
            IsActive = true,
            CreatedAt = SeededAt,
            CreatedBy = SeededBy,
            UpdatedAt = SeededAt,
            UpdatedBy = SeededBy,
        };
}
