using Microsoft.AspNetCore.Identity;

namespace SubastaYa.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string NombreCompleto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}