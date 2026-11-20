using MediatR;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.CreateLabel;

/// <summary>
/// Saves a new label from the designer — its format and its template together (065 FR-001). The
/// body is the designer's <c>{ format, template }</c> request, unchanged (SC-006).
/// </summary>
public sealed record CreateLabelCommand(LabelFormatInput Format, LabelTemplateInput Template)
    : IRequest<Result<LabelResponse>>;
