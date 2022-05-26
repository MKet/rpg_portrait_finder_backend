namespace AuthenticationService.Data.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserByMailAsync(string mail);
        public Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> AddUser(string username, string email, string hash);
    }
}
