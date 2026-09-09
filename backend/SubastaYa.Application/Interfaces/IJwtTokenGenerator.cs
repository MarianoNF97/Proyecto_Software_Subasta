namespace SubastaYa.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(int userId, string email, string nombreCompleto, IList<string> roles);
}