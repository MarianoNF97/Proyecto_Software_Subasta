namespace SubastaYa.Domain.Entities;

public class TransaccionLedger
{
    public int id { get; init; }
    public int billetera_id { get; init; }
    public string tipo { get; init; } = string.Empty;    public decimal monto { get; init; }
    public DateTime fecha { get; init; } = DateTime.UtcNow;
    public int? subasta_id { get; init; }

    public Billetera Billetera { get; set; } = null!;
    public Subasta? Subasta { get; set; }
}
