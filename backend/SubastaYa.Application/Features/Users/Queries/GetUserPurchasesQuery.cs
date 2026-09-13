using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Users.Queries;

public record GetUserPurchasesQuery(int UserId) : IQuery<IEnumerable<AuctionResponseDto>>;