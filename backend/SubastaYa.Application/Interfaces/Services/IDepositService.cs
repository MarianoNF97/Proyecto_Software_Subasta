namespace SubastaYa.Application.Services;

public interface IDepositService
{
    Task<bool> DepositFundsAsync(int userId, decimal amount, CancellationToken cancellationToken = default);
}
