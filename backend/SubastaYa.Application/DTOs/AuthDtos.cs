namespace SubastaYa.Application.DTOs;

public record RegisterRequestDto(
    string Email,
    string Password,
    string NombreCompleto
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    int Id,
    string Email,
    string NombreCompleto,
    string Token
);