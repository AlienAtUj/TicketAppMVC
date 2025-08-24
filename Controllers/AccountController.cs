using System.Linq;
using System.Web.Mvc;
using TicketAppMVC.Models;
using TicketAppMVC.Utils;

namespace TicketAppMVC.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Account/Signup
        [HttpGet]
        public ActionResult Signup() => View();

        [HttpPost]
        public ActionResult Signup(string username, string email, string password, string confirmPassword, string role)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username already exists!";
                return View();
            }

            // Find role in database
            var roleEntity = db.Roles.FirstOrDefault(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.Error = "Invalid role selected!";
                return View();
            }

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                RoleId = roleEntity.Id   // <-- assign RoleId, not Role
            };

            db.Users.Add(user);
            db.SaveChanges();

            // Optional: log in immediately
            Session["User"] = username;
            Session["Role"] = roleEntity.RoleName;

            return RedirectToAction("Index", "Dashboard");
        }


        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string hashedPassword = PasswordHelper.HashPassword(password);
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                Session["User"] = user.Username;
                Session["Role"] = db.Roles.FirstOrDefault(r => r.Id == user.RoleId).RoleName;

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
