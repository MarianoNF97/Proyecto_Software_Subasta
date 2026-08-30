namespace SubastaYa.Domain.Entities;

public class AuditoriaLog
{
    public int id { get; set; }
    public string entidad { get; set; } = string.Empty; //DEFAULT EN VACIO, SUBASTA, BILLETERA, SISTEMA
    public int entidad_id { get; set; }
    public string accion { get; set; } = string.Empty; //DEFAULT EN VACIO, EXTENSION_TIEMPO, CIERRE_WORKER, etc.
    public int? usuario_id { get; set; }
    public string detalle_json { get; set; } = string.Empty;
    public DateTime fecha { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Usuario? Usuario { get; set; }
}