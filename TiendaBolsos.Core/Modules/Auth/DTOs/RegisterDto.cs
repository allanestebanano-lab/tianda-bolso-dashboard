namespace TiendaBolsos.Core.Modules.Auth.DTOs
{
    public class RegisterDto
    {
        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
