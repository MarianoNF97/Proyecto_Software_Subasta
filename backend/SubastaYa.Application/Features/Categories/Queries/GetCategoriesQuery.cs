using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Features.Categories.Queries;

public record GetCategoriesQuery() : IQuery<IEnumerable<CategoryDto>>;
