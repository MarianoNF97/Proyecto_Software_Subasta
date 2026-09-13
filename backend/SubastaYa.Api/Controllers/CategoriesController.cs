using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Categories.Queries;
using SubastaYa.Application.Features.Categories.Queries.Handlers;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(
        [FromServices] GetCategoriesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }
}
