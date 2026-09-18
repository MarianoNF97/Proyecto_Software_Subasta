namespace SubastaYa.Domain.Entities;

public class AuditoriaLog
{
    public int id { get; init; }
    public string entidad { get; init; } = string.Empty;    public int entidad_id { get; init; }
    public string accion { get; init; } = string.Empty;    public int? usuario_id { get; init; }
    public string detalle_json { get; init; } = string.Empty;
    public DateTime fecha { get; init; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
