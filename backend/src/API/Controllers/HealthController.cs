using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Health check and sample test endpoints.
/// Demonstrates MediatR CQRS-Light pattern with the PingCommand.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the HealthController.
    /// </summary>
    /// <param name="mediator">The MediatR mediator for command/query dispatch</param>
    public HealthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Test endpoint that dispatches a PingCommand through MediatR pipeline.
    /// Verifies MediatR integration and handler execution.
    /// </summary>
    /// <returns>HTTP 200 with pong response</returns>
    [HttpPost("ping")]
    public async Task<ActionResult<PingResponse>> Ping()
    {
        var command = new PingCommand();
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}

/// <summary>
/// Response model for ping endpoint.
/// </summary>
public record PingResponse(string Message);
