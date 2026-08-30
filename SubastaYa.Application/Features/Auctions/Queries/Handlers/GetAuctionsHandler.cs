using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;

namespace SubastaYa.Application.Features.Auctions.Queries.Handlers;

public class GetAuctionsHandler : IQueryHandler<GetAuctionsQuery, IEnumerable<AuctionResponseDto>>
{
    private readonly IAuctionRepository _auctionRepository;

    public GetAuctionsHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<IEnumerable<AuctionResponseDto>> HandleAsync(GetAuctionsQuery query, CancellationToken cancellationToken = default)
    {
        var auctions = await _auctionRepository.GetAllAsync(query.Status, query.CategoryId, cancellationToken);

        return auctions.Select(s => new AuctionResponseDto
        {
            Id = s.id,
            SellerId = s.vendedor_id,
            SellerName = s.Vendedor.nombre,
            CategoryId = s.categoria_id,
            CategoryName = s.Categoria.nombre,
            Title = s.titulo,
            Description = s.descripcion,
            ImageUrl = s.url_imagen,
            StartingPrice = s.precio_base,
            MinIncrement = s.incremento_minimo,
            CurrentPrice = s.Pujas.Any() ? s.Pujas.Max(p => p.monto) : s.precio_base,
            TotalBids = s.Pujas.Count,
            StartDate = s.fecha_inicio,
            EndDate = s.fecha_fin,
            Status = s.estado,
            WinningBidderId = s.Pujas.OrderByDescending(p => p.monto).Select(p => (int?)p.comprador_id).FirstOrDefault()
        });
    }
}