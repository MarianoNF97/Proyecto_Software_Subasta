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
            return BadRequest(new { message = "El correo electrónico ya está registrado." });
        }

        var identityUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            NombreCompleto = request.NombreCompleto
        };

        var result = await _userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // Crear entidad de dominio Usuario dejando que la base de datos genere el id autoincremental
        var domainUser = new Usuario
        {
            email = identityUser.Email!,
            nombre = identityUser.NombreCompleto,
            password_hash = identityUser.PasswordHash ?? string.Empty,
            fecha_registro = identityUser.FechaRegistro
        };

        _context.Usuarios.Add(domainUser);
        await _context.SaveChangesAsync();

        // Inicializar billetera vinculada al id generado para el usuario de dominio
        var billetera = new Billetera
        {
            usuario_id = domainUser.id,
            saldo_total = 0,
            saldo_retenido = 0
        };

        _context.Billeteras.Add(billetera);
        await _context.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(identityUser);
        var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

        return Ok(new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var identityUser = await _userManager.FindByEmailAsync(request.Email);
        if (identityUser == null || !await _userManager.CheckPasswordAsync(identityUser, request.Password))
        {
            return Unauthorized(new { message = "Credenciales incorrectas." });
        }

        var domainUser = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.email == identityUser.Email);

        if (domainUser == null)
        {
            return Unauthorized(new { message = "Usuario de dominio no encontrado." });
        }

        var roles = await _userManager.GetRolesAsync(identityUser);
        var token = _jwtTokenGenerator.GenerateToken(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, roles);

        return Ok(new AuthResponseDto(domainUser.id, identityUser.Email!, identityUser.NombreCompleto, token));
    }
}