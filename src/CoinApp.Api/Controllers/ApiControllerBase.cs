using CoinApp.Api.Contracts.Responses;
using CoinApp.Api.Localization;
using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly ILocalizedMessageService _localizedMessageService;

    protected ApiControllerBase(ILocalizedMessageService localizedMessageService)
    {
        _localizedMessageService = localizedMessageService;
    }

    protected IActionResult FromResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Data);
        }

        var messageKey = result.MessageKey ?? result.ErrorCode ?? ServiceErrorCodes.UnexpectedError;
        var response = new ApiErrorResponse(
            result.ErrorCode ?? ServiceErrorCodes.UnexpectedError,
            messageKey,
            _localizedMessageService.Get(messageKey));

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.CoinNotFound, StringComparison.Ordinal))
        {
            return NotFound(response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.UserNotFound, StringComparison.Ordinal))
        {
            return NotFound(response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.AccountNotFound, StringComparison.Ordinal))
        {
            return NotFound(response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.SupportTicketNotFound, StringComparison.Ordinal))
        {
            return NotFound(response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.AuthInvalidCredentials, StringComparison.Ordinal))
        {
            return Unauthorized(response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.UserInactive, StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (string.Equals(result.ErrorCode, ServiceErrorCodes.AuthEmailAlreadyExists, StringComparison.Ordinal))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }
}
