namespace SubastaYa.Application.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message = "Fondos insuficientes en la billetera para realizar la operacion.")
        : base(message) { }
}