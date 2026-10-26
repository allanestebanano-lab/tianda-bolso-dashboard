using Microsoft.EntityFrameworkCore;

using TiendaBolsos.Infrastructure.Data;

using TiendaBolsos.Core.Modules.Auth.Interfaces;

using TiendaBolsos.Core.Modules.Auth.Models;

namespace TiendaBolsos.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(
            string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(x =>
                    x.Correo == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Usuarios.Add(user);

            await _context.SaveChangesAsync();

            return user;
        }
    }
}
