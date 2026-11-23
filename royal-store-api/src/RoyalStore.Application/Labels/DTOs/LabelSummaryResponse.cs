using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Labels.DTOs;

/// <summary>
/// One row of the label listing: name, paper type, label size, columns, status and last change.
/// <see cref="Id"/> is there so a row can be opened, changed or deleted. The format's other
/// measurements and the layout are only returned by <c>GET api/labels/{id}</c>.
/// </summary>
public sealed record LabelSummaryResponse(
    Guid Id,
    string Name,
    LabelPaperType PaperType,
    decimal LabelWidth,
    decimal LabelHeight,
    int Columns,
    bool IsActive,
    DateTimeOffset UpdatedAt);
