using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Services;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions.Commands.Handlers;

public class PlaceBidHandler : ICommandHandler<PlaceBidCommand, BidResponseDto>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IBidValidationService _bidValidationService;
    private readonly IBidPaymentService _bidPaymentService;
    private readonly IAntiSnipingService _antiSnipingService;
    private readonly IAuctionNotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceBidHandler(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IBidValidationService bidValidationService,
        IBidPaymentService bidPaymentService,
        IAntiSnipingService antiSnipingService,
        IAuctionNotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _bidValidationService = bidValidationService;
        _bidPaymentService = bidPaymentService;
        _antiSnipingService = antiSnipingService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BidResponseDto> HandleAsync(PlaceBidCommand command, CancellationToken cancellationToken = default)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        bool extended = false;
        DateTime newEndDate;

        try
        {
            // 1. Validar reglas de negocio y saldo disponible
            await _bidValidationService.ValidateBidAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            // 2. Procesar retención en escrow y liberación del postor previo
            await _bidPaymentService.ProcessBidPaymentAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            // 3. Registrar la puja en base de datos
            var newBid = new Puja
            {
                subasta_id = command.AuctionId,
                comprador_id = command.BuyerId,
                monto = command.Amount,
                fecha_puja = DateTime.UtcNow
            };
            _bidRepository.Add(newBid);

            // 4. Aplicar regla anti-sniping si corresponde
            extended = await _antiSnipingService.ApplyAntiSnipingRuleAsync(command.AuctionId, command.BuyerId, cancellationToken);

            // 5. Confirmar persistencia
            var subasta = await _auctionRepository.GetByIdWithBidsAsync(command.AuctionId, cancellationToken);
            newEndDate = subasta?.fecha_fin ?? DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        // 6. Notificar en tiempo real una vez completada la transacción
        await _notificationService.NotifyNewBidAsync(command.AuctionId, command.Amount, command.BuyerId, cancellationToken);

        if (extended)
        {
            await _notificationService.NotifyTimeExtendedAsync(command.AuctionId, newEndDate, cancellationToken);
        }

        return new BidResponseDto
        {
            Success = true,
            Message = "Puja registrada correctamente.",
            NewAmount = command.Amount,
            TimeExtended = extended,
            NewEndDate = newEndDate
        };
    }
}