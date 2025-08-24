using System.Linq;
using System.Web.Mvc;
using TicketAppMVC.Models;

namespace TicketAppMVC.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard
        public ActionResult Index()
        {
            // Check if user is logged in
            if (Session["User"] == null || Session["Role"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string role = Session["Role"].ToString();

            // Redirect to different dashboards based on role
            if (role == "Student")
            {
                return RedirectToAction("StudentDashboard");
            }
            else
            {
                return RedirectToAction("AdminDashboard"); // Organizer/Admin
            }
        }

        // GET: Dashboard for Student
        public ActionResult StudentDashboard()
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["User"];
            ViewBag.Role = Session["Role"];

            // Fetch all events to display to students
            var events = db.Events.ToList();
            return View(events);  // Pass list of events as model
        }

        // GET: Dashboard for Admin/Organizer
        public ActionResult AdminDashboard()
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["User"];
            ViewBag.Role = Session["Role"];

            // Fetch events created by this organizer/admin
            var organizerUsername = Session["User"].ToString();
            var user = db.Users.FirstOrDefault(u => u.Username == organizerUsername);

            if (user == null)
            {
                ViewBag.Error = "User not found!";
                return View();
            }

            var events = db.Events
                           .Where(e => e.OrganizerId == user.Id)
                           .ToList();

            return View(events);  // Pass events to Admin dashboard view
        }

        // GET: Dashboard/EventDetails/5
        public ActionResult EventDetails(int id)
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            // Load event including the organizer
            var ev = db.Events
                       .Include("Organizer")  // load related User
                       .FirstOrDefault(e => e.Id == id);

            if (ev == null)
                return HttpNotFound();

            return View(ev);  // pass the event to the view
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
