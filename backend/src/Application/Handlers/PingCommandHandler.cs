using MediatR;
using Application.Commands;

namespace Application.Handlers;

/// <summary>
/// Handler for the PingCommand, demonstrating MediatR pipeline integration.
/// This handler processes the ping command and returns a pong response.
/// </summary>
public sealed class PingCommandHandler : IRequestHandler<PingCommand, PingResponse>
{
    /// <summary>
    /// Handles the PingCommand and returns a PingResponse.
    /// </summary>
    /// <param name="request">The PingCommand request</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A PingResponse with message "pong"</returns>
    public Task<PingResponse> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        var response = new PingResponse("pong");
        return Task.FromResult(response);
    }
}
