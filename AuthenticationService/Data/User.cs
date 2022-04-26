namespace AuthenticationService.Data
{
    public class User
    {
        public User(int id, string username, string email, string password)
        {
            Id = id;
            Email = email;
            Password = password;
            Username = username;
        }

        public User(string username, string email, string password)
        {
            Email = email;
            Password = password;
            Username = username;
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }
}
