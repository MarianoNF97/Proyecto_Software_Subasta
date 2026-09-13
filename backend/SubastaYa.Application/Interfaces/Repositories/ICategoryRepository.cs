using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken cancellationToken = default);
}
