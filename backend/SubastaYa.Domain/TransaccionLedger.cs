namespace SubastaYa.Domain.Entities;

public class TransaccionLedger
{
    public int id { get; set; }
    public int billetera_id { get; set; }
    public string tipo { get; set; } = string.Empty; //DEFAULT EN VACIO, PERO TIENE LOS SIGUIENTES TIPOS DE MOV. PERMITIDOS: DEPOSITO, RETENCION, LIBERACION, PAGO, COBRO
    public decimal monto { get; set; }
    public DateTime fecha { get; set; } = DateTime.UtcNow;
    public int? subasta_id { get; set; }

    // Relaciones
    public Billetera Billetera { get; set; } = null!;
    public Subasta? Subasta { get; set; }
}