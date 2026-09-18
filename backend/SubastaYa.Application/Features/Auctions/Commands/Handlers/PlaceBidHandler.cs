using System.Text.Json;
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
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceBidHandler(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IBidValidationService bidValidationService,
        IBidPaymentService bidPaymentService,
        IAntiSnipingService antiSnipingService,
        IAuctionNotificationService notificationService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _bidValidationService = bidValidationService;
        _bidPaymentService = bidPaymentService;
        _antiSnipingService = antiSnipingService;
        _notificationService = notificationService;
        _auditLogRepository = auditLogRepository;
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
            var subasta = await _auctionRepository.GetByIdWithBidsAsync(command.AuctionId, cancellationToken)
                ?? throw new NotFoundException($"No se encontró la subasta con ID {command.AuctionId}.");

            await _bidValidationService.ValidateBidAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            await _bidPaymentService.ProcessBidPaymentAsync(command.AuctionId, command.BuyerId, command.Amount, cancellationToken);

            var newBid = new Puja
            {
                subasta_id = command.AuctionId,
                comprador_id = command.BuyerId,
                monto = command.Amount,
                fecha_puja = ahoraUtc
            };
            _bidRepository.Add(newBid);

            extended = _antiSnipingService.ApplyAntiSnipingRule(subasta, command.BuyerId, ahoraUtc);

            if (!extended)
            {
                subasta.fecha_fin = subasta.fecha_fin.AddMilliseconds(1);
            }
            newEndDate = subasta.fecha_fin;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);

            await RegistrarAuditoriaFalloAsync(
                command.AuctionId,
                command.BuyerId,
                command.Amount,
                "PUJA_RECHAZADA_CONCURRENCIA",
                "Conflicto de concurrencia optimista al procesar la oferta simultánea.",
                cancellationToken);

            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            await RegistrarAuditoriaFalloAsync(
                command.AuctionId,
                command.BuyerId,
                command.Amount,
                "PUJA_RECHAZADA_VALIDACION",
                ex.Message,
                cancellationToken);

            throw;
        }

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

    private async Task RegistrarAuditoriaFalloAsync(
        int subastaId,
        int usuarioId,
        decimal monto,
        string accion,
        string motivoError,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = new AuditoriaLog
            {
                entidad = "SUBASTA",
                entidad_id = subastaId,
                accion = accion,
                usuario_id = usuarioId,
                detalle_json = JsonSerializer.Serialize(new
                {
                    monto_intentado = monto,
                    error = motivoError
                }),
                fecha = DateTime.UtcNow
            };

            _auditLogRepository.Add(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception logEx)
        {
            Console.WriteLine($"[Audit Warning] No se pudo persistir el log de rechazo: {logEx.Message}");
        }
    }
}