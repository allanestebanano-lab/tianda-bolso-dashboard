using TiendaBolsos.Core.Modules.Auth.DTOs;

using TiendaBolsos.Core.Modules.Auth.Interfaces;

using TiendaBolsos.Core.Modules.Auth.Models;

using TiendaBolsos.Shared.JWT;

using TiendaBolsos.Shared.Utilities;

namespace TiendaBolsos.Core.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;

        private readonly JwtGenerator _jwt;

        public AuthService(
            IAuthRepository repository,
            JwtGenerator jwt)
        {
            _repository = repository;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser =
                await _repository.GetByEmailAsync(dto.Correo);

            if (existingUser != null)
            {
                throw new Exception("El usuario ya existe");
            }

            var user = new User
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Rol = "Cajero",
                FechaRegistro = DateTime.UtcNow
            };

            await _repository.CreateAsync(user);

            var token = _jwt.GenerateToken(user.IdUsuario, user.Nombre, user.Rol);

            return new AuthResponseDto
            {
                Token = token,
                Nombre = user.Nombre,
                Correo = user.Correo,
                Rol = user.Rol
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user =
                await _repository.GetByEmailAsync(dto.Correo);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            if (!user.Activo)
            {
                throw new Exception("El usuario está inactivo");
            }

            var validPassword =
                PasswordHasher.Verify(
                    dto.Password,
                    user.PasswordHash);

            if (!validPassword)
            {
                throw new Exception("Contraseña incorrecta");
            }

            var token = _jwt.GenerateToken(user.IdUsuario, user.Nombre, user.Rol);

            return new AuthResponseDto
            {
                Token = token,
                Nombre = user.Nombre,
                Correo = user.Correo,
                Rol = user.Rol
            };
        }
    }
}
