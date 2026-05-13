using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.SupportTickets;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Services.SupportTickets;

public sealed class SupportTicketService : ISupportTicketService
{
    private const int PageSize = 10;

    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ISupportTicketMessageRepository _supportTicketMessageRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public SupportTicketService(
        ISupportTicketRepository supportTicketRepository,
        ISupportTicketMessageRepository supportTicketMessageRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketMessageRepository = supportTicketMessageRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<PaginatedResult<SupportTicketDto>>> GetCurrentUserTicketsAsync(
        int page,
        string? keyword,
        int? status,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<PaginatedResult<SupportTicketDto>>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        if (!TryParseStatus(status, out var ticketStatus))
        {
            return ServiceResult<PaginatedResult<SupportTicketDto>>.Failure(ServiceErrorCodes.SupportTicketInvalidStatus, ServiceErrorCodes.SupportTicketInvalidStatus);
        }

        var query = _supportTicketRepository
            .QueryByUser(_currentUserContext.UserId.Value, keyword, ticketStatus)
            .OrderByDescending(x => x.CreatedAtUtc);

        var result = await _supportTicketRepository.PaginateAsync(query, page, PageSize, cancellationToken);
        var items = result.Items.Select(MapTicket).ToList();

        return ServiceResult<PaginatedResult<SupportTicketDto>>.Success(new PaginatedResult<SupportTicketDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount));
    }

    public async Task<ServiceResult<SupportTicketDto>> CreateAsync(CreateSupportTicketRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<SupportTicketDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        if (!Enum.IsDefined(typeof(SupportTicketType), request.Type))
        {
            return ServiceResult<SupportTicketDto>.Failure(ServiceErrorCodes.SupportTicketInvalidType, ServiceErrorCodes.SupportTicketInvalidType);
        }

        if (!Enum.IsDefined(typeof(SupportTicketPriority), request.Priority))
        {
            return ServiceResult<SupportTicketDto>.Failure(ServiceErrorCodes.SupportTicketInvalidPriority, ServiceErrorCodes.SupportTicketInvalidPriority);
        }

        var message = request.Message.Trim();
        var ticket = new SupportTicket
        {
            UserId = _currentUserContext.UserId.Value,
            Type = (SupportTicketType)request.Type,
            Priority = (SupportTicketPriority)request.Priority,
            Status = SupportTicketStatus.Open,
            Message = message
        };

        var initialMessage = new SupportTicketMessage
        {
            SupportTicketId = ticket.Id,
            SenderUserId = _currentUserContext.UserId.Value,
            Message = message,
            IsStaff = false
        };

        await _supportTicketRepository.AddAsync(ticket, cancellationToken);
        await _supportTicketMessageRepository.AddAsync(initialMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<SupportTicketDto>.Success(MapTicket(ticket));
    }

    public async Task<ServiceResult<SupportTicketThreadDto>> GetThreadAsync(Guid ticketId, int page, CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<SupportTicketThreadDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        var ticket = await _supportTicketRepository.GetThreadForUserAsync(ticketId, _currentUserContext.UserId.Value, cancellationToken);

        if (ticket is null)
        {
            return ServiceResult<SupportTicketThreadDto>.Failure(ServiceErrorCodes.SupportTicketNotFound, ServiceErrorCodes.SupportTicketNotFound);
        }

        var safePage = page < 1 ? 1 : page;
        var messages = ticket.Messages
            .OrderBy(x => x.CreatedAtUtc)
            .Skip((safePage - 1) * PageSize)
            .Take(PageSize)
            .Select(MapMessage)
            .ToList();

        return ServiceResult<SupportTicketThreadDto>.Success(new SupportTicketThreadDto(MapTicket(ticket), messages));
    }

    public async Task<ServiceResult<SupportTicketMessageDto>> ReplyAsync(Guid ticketId, ReplySupportTicketRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<SupportTicketMessageDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        var ticket = await _supportTicketRepository.GetThreadForUserAsync(ticketId, _currentUserContext.UserId.Value, cancellationToken);

        if (ticket is null)
        {
            return ServiceResult<SupportTicketMessageDto>.Failure(ServiceErrorCodes.SupportTicketNotFound, ServiceErrorCodes.SupportTicketNotFound);
        }

        if (ticket.Status == SupportTicketStatus.Closed)
        {
            return ServiceResult<SupportTicketMessageDto>.Failure(ServiceErrorCodes.SupportTicketClosed, ServiceErrorCodes.SupportTicketClosed);
        }

        var message = new SupportTicketMessage
        {
            SupportTicketId = ticket.Id,
            SenderUserId = _currentUserContext.UserId.Value,
            Message = request.Message.Trim(),
            IsStaff = false
        };

        await _supportTicketMessageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<SupportTicketMessageDto>.Success(MapMessage(message));
    }

    private static bool TryParseStatus(int? value, out SupportTicketStatus? status)
    {
        status = null;

        if (!value.HasValue)
        {
            return true;
        }

        if (!Enum.IsDefined(typeof(SupportTicketStatus), value.Value))
        {
            return false;
        }

        status = (SupportTicketStatus)value.Value;
        return true;
    }

    private static SupportTicketDto MapTicket(SupportTicket ticket) =>
        new(
            ticket.Id,
            ticket.Type.ToString().ToLowerInvariant(),
            (int)ticket.Type,
            ticket.Priority.ToString().ToLowerInvariant(),
            (int)ticket.Priority,
            ticket.Status.ToString().ToLowerInvariant(),
            (int)ticket.Status,
            ticket.Message,
            ticket.CreatedAtUtc);

    private static SupportTicketMessageDto MapMessage(SupportTicketMessage message) =>
        new(message.Id, message.Message, message.IsStaff, message.CreatedAtUtc);
}
