using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Services.Commands.DeleteService;

public sealed record DeleteServiceCommand(Guid UserId, Guid ServiceId) : IRequest<Result<Deleted>>;
