using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Domain.Entities;

public class Billetera
{
    public int id { get; set; }
    public int usuario_id { get; set; }
    public decimal saldo_total { get; set; }
    public decimal saldo_retenido { get; set; }
    public decimal saldo_disponible { get; set; } 

    [Timestamp]
    public byte[] version { get; set; } = Array.Empty<byte>();

    // Relaciones
    public Usuario Usuario { get; set; } = null!;
    public ICollection<TransaccionLedger> Transacciones { get; set; } = new List<TransaccionLedger>();
}