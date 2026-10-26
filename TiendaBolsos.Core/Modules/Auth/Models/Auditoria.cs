namespace TiendaBolsos.Core.Modules.Auth.Models
{
    public class Auditoria
    {
        public int IdAuditoria { get; set; }

        public string Tabla { get; set; } = string.Empty;

        public string Accion { get; set; } = string.Empty;

        public string? UsuarioAccion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string? Datos { get; set; }
    }
}