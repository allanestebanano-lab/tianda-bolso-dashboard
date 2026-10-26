namespace TiendaBolsos.Core.Modules.Auth.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}
