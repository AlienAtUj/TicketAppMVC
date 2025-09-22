using System.Web.Mvc;
using TicketAppMVC.Services;
using TicketAppMVC.Models;
using TicketAppMVC.Utils;

namespace TicketAppMVC.Controllers
{
    public class AccountController : Controller
    {
        private AccountService accountService = new AccountService();

        // GET: /Account/Signup
        [HttpGet]
        public ActionResult Signup() => View();

        // POST: /Account/Signup
        [HttpPost]
        public ActionResult Signup(string username, string email, string password, string confirmPassword, string role)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            if (accountService.UsernameExists(username))
            {
                ViewBag.Error = "Username already exists!";
                return View();
            }

            var user = accountService.RegisterUser(username, email, password, role);
            if (user == null)
            {
                ViewBag.Error = "Invalid role selected!";
                return View();
            }

            // Log in immediately
            Session["UserId"] = user.Id;
            Session["User"] = user.Username;
            Session["Role"] = role;

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = accountService.Login(username, password);
            if (user != null)
            {
                Session["UserId"] = user.Id;
                Session["User"] = user.Username;
                Session["Role"] = accountService.GetUserRole(user.Id);

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
