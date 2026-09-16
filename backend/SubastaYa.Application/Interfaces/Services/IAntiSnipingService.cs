using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Services;

public interface IAntiSnipingService
{
    bool ApplyAntiSnipingRule(Subasta subasta, int bidderId, DateTime ahoraUtc);
}