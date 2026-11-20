using System.Text.Json;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Labels;

/// <summary>
/// A label the store saved from the designer (065): what is printed on each label — its layout
/// document — together with the <see cref="LabelFormat"/> it is printed on. The template is the
/// aggregate root and identifies the label; it owns its format one to one (FR-021).
/// </summary>
public sealed class LabelTemplate : Entity, IAuditableEntity
{
    public const int NameMaxLength = 100;
    public const int ConfigurationMaxLength = 65_536;
    public const int SourceTemplateIdMaxLength = 100;

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Trimmed, upper-invariant <see cref="Name"/>; backs the per-store unique index (FR-004).</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public Guid LabelFormatId { get; private set; }
    public LabelFormat Format { get; private set; } = null!;

    /// <summary>
    /// The designer's layout document, stored exactly as sent (SC-001). Only its envelope is checked
    /// (<see cref="IsWellFormedLayout"/>); the elements belong to the designer.
    /// </summary>
    public string Configuration { get; private set; } = string.Empty;

    /// <summary>The designer pre-configuration the label started from. Recorded, never resolved (FR-005).</summary>
    public string? SourceTemplateId { get; private set; }

    /// <summary>At most one per store (database index); never set while inactive (FR-009).</summary>
    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private LabelTemplate() : base(Guid.NewGuid()) { }

    private LabelTemplate(Guid id, Guid tenantId, LabelFormat format) : base(id)
    {
        TenantId = tenantId;
        Format = format;
        LabelFormatId = format.Id;
    }

    /// <summary>The form names are compared in.</summary>
    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    /// <summary>
    /// True when <paramref name="configuration"/> is a JSON object holding an integer
    /// <c>version</c> ≥ 1 and an <c>elements</c> array (possibly empty). Nothing inside the elements
    /// is examined.
    /// </summary>
    public static bool IsWellFormedLayout(string? configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration))
            return false;

        try
        {
            using var document = JsonDocument.Parse(configuration);
            var root = document.RootElement;

            return root.ValueKind == JsonValueKind.Object
                   && root.TryGetProperty("version", out var version)
                   && version.ValueKind == JsonValueKind.Number
                   && version.TryGetInt32(out var number)
                   && number >= 1
                   && root.TryGetProperty("elements", out var elements)
                   && elements.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Every template-level rule the values break, in a fixed order. The request validator reports
    /// all of them (FR-018); the factory and <see cref="Update"/> return the first.
    /// </summary>
    public static IReadOnlyList<Error> Check(
        string? name, string? configuration, string? sourceTemplateId, bool isDefault, bool isActive)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(Error.Validation("label.name_required", "Label name is required."));
        else if (name.Trim().Length > NameMaxLength)
            errors.Add(Error.Validation("label.name_too_long",
                $"Label name must not exceed {NameMaxLength} characters."));

        if (string.IsNullOrWhiteSpace(configuration))
            errors.Add(Error.Validation("label.layout_required", "Label layout (configuration) is required."));
        else if (configuration.Length > ConfigurationMaxLength)
            errors.Add(Error.Validation("label.layout_too_large",
                $"Label layout (configuration) must not exceed {ConfigurationMaxLength} characters."));
        else if (!IsWellFormedLayout(configuration))
            errors.Add(Error.Validation("label.layout_invalid",
                "Label layout (configuration) must be a JSON object with an integer \"version\" of at least 1 and an \"elements\" array."));

        if (sourceTemplateId is not null && sourceTemplateId.Trim().Length > SourceTemplateIdMaxLength)
            errors.Add(Error.Validation("label.source_too_long",
                $"Source template id must not exceed {SourceTemplateIdMaxLength} characters."));

        if (isDefault && !isActive)
            errors.Add(Error.Validation("label.inactive_cannot_be_default",
                "An inactive label cannot be the default."));

        return errors;
    }

    public static Result<LabelTemplate> Create(
        Guid tenantId,
        string name,
        LabelFormatSpec format,
        string configuration,
        string? sourceTemplateId,
        bool isDefault,
        bool isActive)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("label.tenant_required", "Tenant identifier is required.");

        var errors = Check(name, configuration, sourceTemplateId, isDefault, isActive);
        if (errors.Count > 0)
            return errors[0];

        var createdFormat = LabelFormat.Create(tenantId, format, isActive);
        if (createdFormat.IsFailure)
            return createdFormat.Error;

        var template = new LabelTemplate(Guid.NewGuid(), tenantId, createdFormat.Value);
        template.Apply(name, configuration, sourceTemplateId, isDefault, isActive);
        return template;
    }

    /// <summary>
    /// Replaces every field of the label and its format (FR-012). Everything is checked before
    /// anything changes, so a refused change leaves the label as it was.
    /// </summary>
    public Result Update(
        string name,
        LabelFormatSpec format,
        string configuration,
        string? sourceTemplateId,
        bool isDefault,
        bool isActive)
    {
        var errors = Check(name, configuration, sourceTemplateId, isDefault, isActive);
        if (errors.Count > 0)
            return Result.Failure(errors[0]);

        // Validates the format before touching it; the template's own rules passed above.
        var updated = Format.Update(format);
        if (updated.IsFailure)
            return updated;

        Apply(name, configuration, sourceTemplateId, isDefault, isActive);
        Format.SetActive(isActive);
        return Result.Success();
    }

    /// <summary>Idempotent (spec US4 scenario 3).</summary>
    public void Activate()
    {
        IsActive = true;
        Format.SetActive(true);
    }

    /// <summary>Idempotent. An inactive label cannot be the default, so the mark goes too (FR-014).</summary>
    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        Format.SetActive(false);
    }

    /// <summary>Clears the default mark — the first of the two saves of a default swap (065 research D7).</summary>
    public void UnmarkDefault() => IsDefault = false;

    private void Apply(string name, string configuration, string? sourceTemplateId, bool isDefault, bool isActive)
    {
        Name = name.Trim();
        NormalizedName = Normalize(name);
        Configuration = configuration;
        SourceTemplateId = string.IsNullOrWhiteSpace(sourceTemplateId) ? null : sourceTemplateId.Trim();
        IsDefault = isDefault;
        IsActive = isActive;
    }
}
