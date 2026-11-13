namespace RoyalStore.Application.Abstractions.Data;

/// <summary>
/// Tells whether a persistence failure was a unique-constraint violation.
/// </summary>
/// <remarks>
/// Exists so handlers can treat "someone already inserted this row" as an expected outcome without
/// the Application layer knowing which database produced the error — the provider-specific SQLSTATE
/// check lives in Infrastructure, where the provider does (Constitution I).
/// </remarks>
public interface IUniqueConstraintDetector
{
    bool IsUniqueViolation(Exception exception);
}
