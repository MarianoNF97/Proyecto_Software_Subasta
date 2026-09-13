using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.DTOs;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Identity;
using SubastaYa.Infrastructure.Persistence;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { error = "El correo electrónico ya está registrado." });
        }

        // Paso 1.4: Transacción atómica que asegura Identity + Dominio + Billetera
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Crear usuario en Identity
            var identityUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NombreCompleto = request.NombreCompleto
            };

            var result = await _userManager.CreateAsync(identityUser, request.Password);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            // 2. Crear entidad de dominio Usuario vinculada
            var domainUser = new Usuario
            {
                email = identityUser.Email!,
                nombre = identityUser.NombreCompleto,
                password_hash = identityUser.PasswordHash ?? string.Empty,
                fecha_registro = identityUser.FechaRegistro
            };

            _context.Usuarios.Add(domainUser);
            await _context.SaveChangesAsync();

            // 3. Inicializar billetera vinculada al id del usuario de dominio
            var billetera = new Billetera
            {
                usuario_id = domainUser.id,
                saldo_total = 0,
                saldo_retenido = 0
            };

            _context.Billeteras.Add(billetera);
            await _context.SaveChangesAsync();

            // Confirmar transacción completa
            await transaction.CommitAsync();

            var roles = await _userManager.GetRolesAsync(identityUser);
            var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

            return Ok(new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var identityUser = await _userManager.FindByEmailAsync(request.Email);
        if (identityUser == null || !await _userManager.CheckPasswordAsync(identityUser, request.Password))
        {
            return Unauthorized(new { error = "Credenciales incorrectas." });
        }

        var domainUser = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.email == identityUser.Email);

        if (domainUser == null)
        {
            return Unauthorized(new { error = "Usuario de dominio no encontrado." });
        }

        var roles = await _userManager.GetRolesAsync(identityUser);
        var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

        return Ok(new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token));
    }
}