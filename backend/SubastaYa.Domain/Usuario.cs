namespace SubastaYa.Domain.Entities;

public class Usuario
{
    public int id { get; set; }
    public string email { get; set; } = string.Empty;
    public string nombre { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public DateTime fecha_registro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Billetera? Billetera { get; set; }
    public ICollection<Subasta> SubastasCreadas { get; set; } = new List<Subasta>();
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
    public ICollection<AuditoriaLog> AuditoriaLogs { get; set; } = new List<AuditoriaLog>();
}