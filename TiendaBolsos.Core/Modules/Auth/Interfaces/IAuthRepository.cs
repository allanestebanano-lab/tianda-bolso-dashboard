using TiendaBolsos.Core.Modules.Auth.Models;

namespace TiendaBolsos.Core.Modules.Auth.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User> CreateAsync(User user);
    }
}
