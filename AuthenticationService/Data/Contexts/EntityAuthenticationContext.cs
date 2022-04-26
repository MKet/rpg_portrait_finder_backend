using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;

namespace AuthenticationService.Data.Contexts
{
    public class EntityAuthenticationContext : DbContext
    {
        public EntityAuthenticationContext(DbContextOptions options) : base(options)
        {
            Users = Set<User>();
        }

        public DbSet<User> Users { get; set; }

        public async Task SeedAsync()
        {
            await Users.AddRangeAsync(
                new User("bob322", "bob@example.net", HashPassword("password", 12, true)),
                new User("Syllian", "koolraap@example.net", HashPassword("password", 12, true)),
                new User("Devon", "devon@example.net", HashPassword("password", 12, true))
            );
            await SaveChangesAsync();
        }
    }
}
