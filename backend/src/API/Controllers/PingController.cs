using API.Authorization;
using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Sample test endpoint for MediatR CQRS-Light verification.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the PingController.
    /// </summary>
    /// <param name="mediator">The MediatR mediator for command/query dispatch.</param>
    public PingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Test endpoint that dispatches a PingCommand through MediatR pipeline.
    /// </summary>
    /// <returns>HTTP 200 with ping response payload.</returns>
    [HttpPost]
    [Authorize(Policy = RoleRequirements.StaffPolicy)]
    public async Task<ActionResult<PingResponse>> Ping()
    {
        var command = new PingCommand();
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("patient")]
    [Authorize(Policy = RoleRequirements.PatientPolicy)]
    public IActionResult PatientPing() =>
        Ok(new { status = "ok", scope = "patient" });

    [HttpGet("staff")]
    [Authorize(Policy = RoleRequirements.StaffPolicy)]
    public IActionResult StaffPing() =>
        Ok(new { status = "ok", scope = "staff" });

    [HttpGet("admin")]
    [Authorize(Policy = RoleRequirements.AdminPolicy)]
    public IActionResult AdminPing() =>
        Ok(new { status = "ok", scope = "admin" });
}