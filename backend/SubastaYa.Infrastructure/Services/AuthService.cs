using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Services;
using SubastaYa.Application.Exceptions;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Application.Common.Interfaces;

namespace SubastaYa.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BusinessValidationException("El correo electrónico ya está registrado.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var identityUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NombreCompleto = request.NombreCompleto
            };

            var result = await _userManager.CreateAsync(identityUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessValidationException(errors);
            }

            var domainUser = new Usuario
            {
                email = identityUser.Email!,
                nombre = identityUser.NombreCompleto,
                password_hash = identityUser.PasswordHash ?? string.Empty,
                fecha_registro = identityUser.FechaRegistro
            };

            _context.Usuarios.Add(domainUser);
            await _context.SaveChangesAsync(cancellationToken);

            var billetera = new Billetera
            {
                usuario_id = domainUser.id,
                saldo_total = 0,
                saldo_retenido = 0
            };

            _context.Billeteras.Add(billetera);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var roles = await _userManager.GetRolesAsync(identityUser);
            var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

            return new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var identityUser = await _userManager.FindByEmailAsync(request.Email);
        if (identityUser == null || !await _userManager.CheckPasswordAsync(identityUser, request.Password))
        {
            throw new UnauthorizedAccessException("Credenciales incorrectas.");
        }

        var domainUser = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.email == identityUser.Email, cancellationToken);

        if (domainUser == null)
        {
            throw new UnauthorizedAccessException("Usuario de dominio no encontrado.");
        }

        var roles = await _userManager.GetRolesAsync(identityUser);
        var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

        return new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token);
    }
}
