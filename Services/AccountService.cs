using System.Linq;
using TicketAppMVC.Models;
using TicketAppMVC.Utils;

namespace TicketAppMVC.Services
{
    public class AccountService
    {
        private readonly ApplicationDbContext db;

        public AccountService()
        {
            db = new ApplicationDbContext();
        }

        // Check if username exists
        public bool UsernameExists(string username)
        {
            var query = from u in db.Users
                        where u.Username == username
                        select u;
            return query.Any();
        }

        // Get role by name
        public Role GetRoleByName(string roleName)
        {
            var query = from r in db.Roles
                        where r.RoleName == roleName
                        select r;
            return query.FirstOrDefault();
        }

        // Register new user
        public User RegisterUser(string username, string email, string password, string roleName)
        {
            var role = GetRoleByName(roleName);
            if (role == null) return null;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                RoleId = role.Id
            };

            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

        // Validate login
        public User Login(string username, string password)
        {
            string hashedPassword = PasswordHelper.HashPassword(password);

            var query = from u in db.Users
                        where u.Username == username && u.PasswordHash == hashedPassword
                        select u;

            return query.FirstOrDefault();
        }

        // Get role of user
        public string GetUserRole(int userId)
        {
            var query = from u in db.Users
                        join r in db.Roles on u.RoleId equals r.Id
                        where u.Id == userId
                        select r.RoleName;

            return query.FirstOrDefault();
        }
    }
}
