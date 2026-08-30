namespace SubastaYa.Domain.Entities;

public class Puja
{
    public int id { get; set; }
    public int subasta_id { get; set; }
    public int comprador_id { get; set; }
    public decimal monto { get; set; }
    public DateTime fecha_puja { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Subasta Subasta { get; set; } = null!;
    public Usuario Comprador { get; set; } = null!;
}