using AuthenticationService.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly EntityAuthenticationContext _context;

        public UserRepository(EntityAuthenticationContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await (from user in _context.Users
                          where user.Id == id
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByMailAsync(string mail)
        {
            return await (from user in _context.Users
                         where user.Email == mail
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await (from user in _context.Users
                          where user.Username == username
                          select user).FirstOrDefaultAsync();
        }
    }
}
