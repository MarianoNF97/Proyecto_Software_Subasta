using SubastaYa.Application.DTOs;
using SubastaYa.Application.Exceptions;
using SubastaYa.Application.Features.Auctions.Commands;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions.Commands.Handlers;

public class CreateAuctionHandler : ICommandHandler<CreateAuctionCommand, AuctionResponseDto>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAuctionHandler(IAuctionRepository auctionRepository, IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuctionResponseDto> HandleAsync(CreateAuctionCommand command, CancellationToken cancellationToken = default)
    {
        if (command.EndDate <= command.StartDate)
            throw new BusinessValidationException("La fecha de fin debe ser posterior a la fecha de inicio.");

        if (command.StartingPrice <= 0 || command.MinIncrement <= 0)
            throw new BusinessValidationException("El precio base y el incremento minimo deben ser valores positivos.");

        var subasta = new Subasta
        {
            vendedor_id = command.SellerId,
            categoria_id = command.CategoryId,
            titulo = command.Title,
            descripcion = command.Description,
            url_imagen = command.ImageUrl,
            precio_base = command.StartingPrice,
            incremento_minimo = command.MinIncrement,
            fecha_inicio = command.StartDate,
            fecha_fin = command.EndDate,
            estado = command.StartDate <= DateTime.UtcNow ? "ACTIVA" : "PROGRAMADA"
        };

        await _auctionRepository.AddAsync(subasta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuctionResponseDto
        {
            Id = subasta.id,
            SellerId = subasta.vendedor_id,
            CategoryId = subasta.categoria_id,
            Title = subasta.titulo,
            Description = subasta.descripcion,
            ImageUrl = subasta.url_imagen,
            StartingPrice = subasta.precio_base,
            MinIncrement = subasta.incremento_minimo,
            CurrentPrice = subasta.precio_base,
            TotalBids = 0,
            StartDate = subasta.fecha_inicio,
            EndDate = subasta.fecha_fin,
            Status = subasta.estado,
            WinningBidderId = null
        };
    }
}