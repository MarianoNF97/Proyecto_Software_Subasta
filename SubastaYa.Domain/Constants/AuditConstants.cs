namespace SubastaYa.Domain.Constants;

/// <summary>
/// Constantes relacionadas con auditoría de operaciones.
/// </summary>
public static class AuditConstants
{
    // Entidades
    public const string ENTIDAD_SUBASTA = "SUBASTA";
    public const string ENTIDAD_BILLETERA = "BILLETERA";
    public const string ENTIDAD_SISTEMA = "SISTEMA";

    // Acciones
    public const string ACCION_CIERRE_DESIERTA = "CIERRE_DESIERTA";
    public const string ACCION_CIERRE_FINALIZADA = "CIERRE_FINALIZADA";
    public const string ACCION_EXTENSION_TIEMPO = "EXTENSION_TIEMPO";
    public const string ACCION_CARGA_SALDO = "CARGA_SALDO";
}
