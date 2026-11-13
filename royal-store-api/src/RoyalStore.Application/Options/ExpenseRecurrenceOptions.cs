using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

/// <summary>Scheduling of the recurring-expense generator (060 FR-031).</summary>
public sealed class ExpenseRecurrenceOptions
{
    public const string SectionName = "ExpenseRecurrence";

    /// <summary>
    /// Set false to stop the scheduled run. The manual trigger endpoint keeps working, so a store
    /// can still generate on demand while the schedule is off.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Hour (UTC) of the daily run. Defaults to a low-traffic hour.</summary>
    [Range(0, 23)]
    public int RunHourUtc { get; init; } = 2;

    /// <summary>
    /// How often the worker wakes to check whether the run hour has arrived. Short enough that a
    /// deploy mid-day does not skip the day's run, long enough to be nearly free.
    /// </summary>
    [Range(1, 1440)]
    public int PollIntervalMinutes { get; init; } = 15;
}
