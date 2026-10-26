using TiendaBolsos.Core.Modules.Auth.DTOs;

namespace TiendaBolsos.Core.Modules.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    }
}
