using MediatR;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.UpdateLabel;

/// <summary>
/// Replaces every field of a stored label and its format (065 FR-012). <see cref="Id"/> is the
/// template id from the route; the body is the same <c>{ format, template }</c> as on creation, with
/// the label's own format id.
/// </summary>
public sealed record UpdateLabelCommand(Guid Id, LabelFormatInput Format, LabelTemplateInput Template)
    : IRequest<Result<LabelResponse>>;
