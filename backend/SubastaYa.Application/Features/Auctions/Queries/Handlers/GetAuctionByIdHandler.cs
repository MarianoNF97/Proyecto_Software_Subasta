using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Application.Features.Auctions.Queries;

namespace SubastaYa.Application.Features.Auctions.Queries.Handlers;

public class GetAuctionByIdHandler : IQueryHandler<GetAuctionByIdQuery, AuctionResponseDto?>
{
    private readonly IAuctionRepository _auctionRepository;

    public GetAuctionByIdHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<AuctionResponseDto?> HandleAsync(GetAuctionByIdQuery query, CancellationToken cancellationToken = default)
    {
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (auction == null) return null;

        var highestBid = auction.Pujas.OrderByDescending(p => p.monto).FirstOrDefault();
        var currentPrice = highestBid != null ? highestBid.monto : auction.precio_base;
        var nextMinimumBid = highestBid != null ? currentPrice + auction.incremento_minimo : auction.precio_base;

        return new AuctionResponseDto
        {
            Id = auction.id,
            SellerId = auction.vendedor_id,
            SellerName = auction.Vendedor?.nombre ?? "Vendedor",
            CategoryId = auction.categoria_id,
            CategoryName = auction.Categoria?.nombre ?? "General",
            Title = auction.titulo,
            Description = auction.descripcion,
            ImageUrl = auction.url_imagen,
            StartingPrice = auction.precio_base,
            MinIncrement = auction.incremento_minimo,
            CurrentPrice = currentPrice,
            NextMinimumBid = nextMinimumBid,
            TotalBids = auction.Pujas.Count,
            StartDate = DateTime.SpecifyKind(auction.fecha_inicio, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(auction.fecha_fin, DateTimeKind.Utc),
            Status = auction.estado,
            WinningBidderId = highestBid?.comprador_id,
            Bids = auction.Pujas
                .OrderByDescending(p => p.fecha_puja)
                .Select(p => new BidItemDto
                {
                    Id = p.id,
                    BuyerId = p.comprador_id,
                    BuyerName = p.Comprador?.nombre ?? $"Usuario #{p.comprador_id}",
                    Amount = p.monto,
                    Time = DateTime.SpecifyKind(p.fecha_puja, DateTimeKind.Utc)
                })
                .ToList()
        };
    }
}