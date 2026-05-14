using MediatR;

namespace Application.Commands;

/// <summary>
/// Sample command demonstrating MediatR CQRS-Light pattern.
/// This command is processed through the MediatR pipeline.
/// </summary>
public sealed record PingCommand : IRequest<PingResponse>;

/// <summary>
/// Response from the PingCommand handler.
/// </summary>
public sealed record PingResponse(string Message);
