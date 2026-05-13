using CoinApp.Api.Localization;
using CoinApp.Application.Dtos.SupportTickets;
using CoinApp.Application.Services.SupportTickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Authorize]
[Route("api/tickets")]
public sealed class SupportTicketsController : ApiControllerBase
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketsController(ISupportTicketService supportTicketService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _supportTicketService = supportTicketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets(
        [FromQuery] int page = 1,
        [FromQuery] string? keyword = null,
        [FromQuery] int? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _supportTicketService.GetCurrentUserTicketsAsync(page, keyword, status, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _supportTicketService.CreateAsync(request, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetThread([FromRoute] Guid ticketId, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _supportTicketService.GetThreadAsync(ticketId, page, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{ticketId:guid}/reply")]
    public async Task<IActionResult> Reply([FromRoute] Guid ticketId, [FromBody] ReplySupportTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _supportTicketService.ReplyAsync(ticketId, request, cancellationToken);
        return FromResult(result);
    }
}
