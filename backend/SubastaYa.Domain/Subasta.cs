using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Domain.Entities;

public class Subasta
{
    public int id { get; set; }
    public int vendedor_id { get; set; }
    public int categoria_id { get; set; }
    public string titulo { get; set; } = string.Empty;
    public string descripcion { get; set; } = string.Empty;
    public string url_imagen { get; set; } = string.Empty;
    public decimal precio_base { get; set; }
    public decimal incremento_minimo { get; set; }
    public DateTime fecha_inicio { get; set; }
    public DateTime fecha_fin { get; set; }
    public string estado { get; set; } = "PROGRAMADA"; // POR DEFAULT ESTA EN ESTADO PROGRAMADA, SE CAMBIA A EN_CURSO CUANDO SE INICIA LA SUBASTA Y A FINALIZADA CUANDO SE TERMINA

    [Timestamp]
    public byte[] version { get; set; } = Array.Empty<byte>();

    // Relaciones
    public Usuario Vendedor { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
    public ICollection<TransaccionLedger> TransaccionesLedger { get; set; } = new List<TransaccionLedger>();
}