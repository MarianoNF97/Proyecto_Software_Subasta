using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Features.Wallets.Commands;
using SubastaYa.Application.Features.Wallets.Commands.Handlers;
using SubastaYa.Application.Features.Wallets.Queries;
using SubastaYa.Application.Features.Wallets.Queries.Handlers;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance(
        int userId,
        [FromServices] GetWalletBalanceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetWalletBalanceQuery(userId), cancellationToken);
        if (result == null)
            return NotFound(new { mensaje = $"Billetera no encontrada para el usuario con ID {userId}." });

        return Ok(result);
    }

    [HttpGet("{userId:int}/transactions")]
    public async Task<ActionResult<IEnumerable<LedgerTransactionDto>>> GetTransactions(
        int userId,
        [FromServices] GetWalletTransactionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetWalletTransactionsQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("deposit")]
    public async Task<ActionResult> Deposit(
        [FromBody] DepositFundsCommand command,
        [FromServices] DepositFundsHandler handler,
        CancellationToken cancellationToken)
    {
        var success = await handler.HandleAsync(command, cancellationToken);
        return Ok(new { success, mensaje = "Depósito acreditado correctamente en la billetera." });
    }
}