namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>One week of the projection. The three buckets never contain the same obligation twice.</summary>
/// <param name="Committed">Bills that already exist and fall due this week.</param>
/// <param name="Projected">
/// Cycles a template will generate but has not yet. Every cycle already created appears under
/// <paramref name="Committed"/> and is excluded here, so nothing is counted twice (FR-046).
/// </param>
public record CashFlowWeek(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    decimal Overdue,
    decimal Committed,
    decimal Projected);

/// <summary>A headline figure split by how certain it is.</summary>
public record CashFlowBucket(decimal Total, decimal Committed, decimal Projected);

/// <summary>Cash-flow projection over the weeks ahead (FR-045, FR-047).</summary>
public record CashFlowProjectionResponse(
    decimal Overdue,
    CashFlowBucket Next30Days,
    CashFlowBucket FullHorizon,
    IReadOnlyList<CashFlowWeek> Weeks);
