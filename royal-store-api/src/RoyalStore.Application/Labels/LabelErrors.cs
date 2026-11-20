using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels;

/// <summary>Application-level label refusals (065 contracts/labels-api.md).</summary>
public static class LabelErrors
{
    public static readonly Error NotFound =
        Error.NotFound("label.not_found", "Label was not found.");

    public static readonly Error NameTaken =
        Error.Conflict("label.name_taken", "Another label of this store already has this name.");

    public static readonly Error DefaultConflict =
        Error.Conflict("label.default_conflict",
            "The default label was changed by someone else at the same time. Reload and try again.");

    public static readonly Error FormatMismatch =
        Error.Validation("label.format_mismatch", "The format sent is not this label's format.");

    /// <summary>
    /// Which invariant a failed save tripped (065 research D6, D7), or null when the failure is not
    /// one of the two the label indexes guard.
    /// </summary>
    public static Error? FromUniqueViolation(IUniqueConstraintDetector detector, Exception exception)
    {
        if (detector.IsUniqueViolation(exception, LabelIndexNames.Name))
            return NameTaken;

        if (detector.IsUniqueViolation(exception, LabelIndexNames.Default))
            return DefaultConflict;

        return null;
    }
}
