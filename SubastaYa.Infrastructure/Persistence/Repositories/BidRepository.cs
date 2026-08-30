using SubastaYa.Application.Interfaces.Repositories;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class BidRepository : IBidRepository
{
    private readonly ApplicationDbContext _context;

    public BidRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Puja bid)
    {
        _context.Pujas.Add(bid);
    }
}