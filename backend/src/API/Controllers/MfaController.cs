using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MfaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtTokenService _tokenService;

    public MfaController(IMediator mediator, IJwtTokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> Setup([FromBody] SetupMfaCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return result.Status switch
            {
                "phone_required" => BadRequest(new { code = result.Status, message = result.Message }),
                "account_disabled" => StatusCode(StatusCodes.Status423Locked, new { code = result.Status, message = result.Message }),
                "not_required" => BadRequest(new { code = result.Status, message = result.Message }),
                "not_found" => NotFound(new { code = result.Status, message = result.Message }),
                _ => BadRequest(new { code = result.Status, message = result.Message }),
            };
        }

        return Ok(new
        {
            status = result.Status,
            message = result.Message,
            user = new { email = result.Email, role = result.Role.ToString().ToLowerInvariant() },
            method = result.Method.ToString().ToLowerInvariant(),
            manualKey = result.ManualKey,
            qrCodeUri = result.QrCodeUri,
            phoneNumber = result.PhoneNumber,
            expiresAtUtc = result.ExpiresAtUtc,
        });
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify([FromBody] VerifyMfaCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return result.Status switch
            {
                "locked" => StatusCode(StatusCodes.Status423Locked, new { code = result.Status, message = result.Message, attemptsRemaining = result.AttemptsRemaining }),
                "expired" => StatusCode(StatusCodes.Status410Gone, new { code = result.Status, message = result.Message, attemptsRemaining = result.AttemptsRemaining, canResend = true }),
                "account_disabled" => StatusCode(StatusCodes.Status423Locked, new { code = result.Status, message = result.Message }),
                "method_mismatch" => BadRequest(new { code = result.Status, message = result.Message }),
                "setup_required" => BadRequest(new { code = result.Status, message = result.Message }),
                _ => Unauthorized(new { code = result.Status, message = result.Message, attemptsRemaining = result.AttemptsRemaining }),
            };
        }

        var token = await _tokenService.IssueTokenAsync(
                new AuthTokenPayload(result.UserId, result.Email, result.Role.ToString().ToLowerInvariant()),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(new
        {
            accessToken = token.AccessToken,
            expiresAtUtc = token.ExpiresAtUtc,
            tokenType = token.TokenType,
            user = new { email = result.Email, role = result.Role.ToString().ToLowerInvariant() },
        });
    }

    [HttpPost("request-code")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestCode([FromBody] RequestMfaCodeCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return result.Status switch
            {
                "sms_unavailable" => BadRequest(new { code = result.Status, message = result.Message }),
                "not_found" => NotFound(new { code = result.Status, message = result.Message }),
                _ => BadRequest(new { code = result.Status, message = result.Message }),
            };
        }

        return Ok(new
        {
            status = result.Status,
            message = result.Message,
            user = new { email = result.Email, role = result.Role.ToString().ToLowerInvariant() },
            method = result.Method.ToString().ToLowerInvariant(),
            attemptsRemaining = result.AttemptsRemaining,
            expiresAtUtc = result.ExpiresAtUtc,
        });
    }
}