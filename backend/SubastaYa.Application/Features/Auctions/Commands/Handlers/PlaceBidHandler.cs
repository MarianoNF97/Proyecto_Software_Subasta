using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Exceptions;
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
        var ahoraUtc = DateTime.UtcNow;

        try
        {
            // 1. Obtener la subasta en memoria
            var subasta = await _auctionRepository.GetByIdWithBidsAsync(command.AuctionId, cancellationToken)
                ?? throw new NotFoundException($"No se encontró la subasta con ID {command.AuctionId}.");

            // 2. Validar reglas de negocio y saldo disponible
            await _bidValidationService.ValidateBidAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            // 3. Procesar retención en escrow y liberación del postor previo
            await _bidPaymentService.ProcessBidPaymentAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            // 4. Registrar la puja en base de datos
            var newBid = new Puja
            {
                subasta_id = command.AuctionId,
                comprador_id = command.BuyerId,
                monto = command.Amount,
                fecha_puja = ahoraUtc
            };
            _bidRepository.Add(newBid);

            // 5. Aplicar regla anti-sniping si corresponde
            extended = _antiSnipingService.ApplyAntiSnipingRule(subasta, command.BuyerId, ahoraUtc);

            // 6. Concurrencia Optimista:
            
            if (!extended)
            {
                subasta.fecha_fin = subasta.fecha_fin.AddMilliseconds(1);
            }
            newEndDate = subasta.fecha_fin;

            // 7. Confirmar persistencia atómica
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        // 8. Notificar en tiempo real una vez completada la transacción
        await _notificationService.NotifyNewBidAsync(command.AuctionId, command.Amount, command.BuyerId, cancellationToken);

        if (extended)
        {
            await _notificationService.NotifyTimeExtendedAsync(command.AuctionId, newEndDate, cancellationToken);
        }

        return new BidResponseDto
        {
            Success = true,
            Message = extended
                ? "Puja registrada. El tiempo de la subasta fue extendido por regla anti-sniping."
                : "Puja registrada correctamente.",
            NewAmount = command.Amount,
            TimeExtended = extended,
            NewEndDate = DateTime.SpecifyKind(newEndDate, DateTimeKind.Utc)
        };
    }
}