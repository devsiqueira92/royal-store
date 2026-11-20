namespace RoyalStore.Application.Labels;

/// <summary>
/// Names of the two unique indexes that guard label invariants (065 research D6, D7). Declared here
/// so the handlers can tell which one a failed save tripped; the EF configuration uses the same names.
/// </summary>
public static class LabelIndexNames
{
    /// <summary>Per-store unique name → <c>label.name_taken</c>.</summary>
    public const string Name = "IX_LabelTemplates_TenantId_NormalizedName";

    /// <summary>At most one default per store → <c>label.default_conflict</c>.</summary>
    public const string Default = "IX_LabelTemplates_TenantId_Default";
}
