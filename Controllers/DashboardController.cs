using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TicketAppMVC.Models;
using TicketAppMVC.Services;

namespace TicketAppMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService service = new DashboardService();
        private ApplicationDbContext db = new ApplicationDbContext();

   
        public ActionResult Index()
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            string role = Session["Role"].ToString();

            if (role == "Student")
                return RedirectToAction("StudentDashboard");

            return RedirectToAction("OrganizerDashboard");
        }

        public ActionResult OrganizerDashboard()
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["User"];
            ViewBag.Role = Session["Role"];

            
            var organizerEvents = service.GetEventsForOrganizer(Convert.ToInt32(Session["UserId"]));

            return View(organizerEvents); 
        }

        public ActionResult StudentDashboard()
        {
            if (Session["User"] == null || Session["Role"] == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["User"];
            ViewBag.Role = Session["Role"];

            List<Event> events = service.GetAllEvents();
            return View(events);
        }

        [HttpPost]
        public ActionResult AddToCart(int eventId, int? ticketTypeId, int quantity)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "You must be logged in to add tickets.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var ev = service.GetEventDetails(eventId);
            if (ev == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("StudentDashboard");
            }

            service.AddToCart(userId, eventId, ticketTypeId, quantity, ev.Price);

            TempData["Success"] = "Tickets added to cart!";
            return RedirectToAction("CartDashboard");
        }

        // In your DashboardController.cs, add this action
        public ActionResult MyTickets()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                List<TicketViewModel> userTickets = service.GetUserTickets(userId);
                return View(userTickets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading your tickets. Please try again.";
                Console.WriteLine($"Error loading tickets: {ex.Message}");
                return View(new List<TicketViewModel>());
            }
        }

        public ActionResult CartDashboard()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            // <-- HERE is where we call it
            var cartItems = service.GetCartSessionItems(userId);

            return View(cartItems);
        }

        public ActionResult DeleteCart(string eventTitle)
        {
            

            bool success = service.DeleteCartItem(eventTitle);

            if (success)
            {
                
                return new HttpStatusCodeResult(200);
            }
            else
            {
               
                return new HttpStatusCodeResult(400);
            }
        }

        [HttpPost]
       
        public ActionResult UpdateCartQuantities(List<CartItemSession> cartItems)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            Console.WriteLine($"Controller: Received {cartItems.Count} items to update for UserId: {userId}");

            foreach (var item in cartItems)
            {
                Console.WriteLine($"Controller sees CartItem -> CartItemId: {item.CartItemId}, EventTitle: {item.EventTitle}, Quantity: {item.Quantity}, Price: {item.Price}");
            }

            // Call service to update
            service.UpdateCartItems(userId, cartItems);

            return RedirectToAction("CartDashboard");
        }


        public ActionResult EventDetails(int id)
        {
            if (id <= 0)
                return HttpNotFound();

            Event ev = service.EventDetailIndex(id);
            if (ev == null)
                return HttpNotFound();

            return View(ev);
        }

        //Processing payment
        [HttpPost]
        public ActionResult ProcessPayment()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessPayment] Starting for UserId = {userId}");

                bool paymentSuccess = service.ProcessPayment(userId);

                System.Diagnostics.Debug.WriteLine($"[ProcessPayment] Result = {paymentSuccess}");

                if (paymentSuccess)
                {
                    TempData["Success"] = "Payment processed successfully! Your tickets have been generated.";
                    return RedirectToAction("MyTickets", "Dashboard");
                }
                else
                {
                    TempData["Error"] = "Payment failed. Please try again or contact support.";
                    return RedirectToAction("CartDashboard");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessPayment] Exception: {ex}");
                TempData["Error"] = $"An error occurred during payment processing: {ex.Message}";
                return RedirectToAction("CartDashboard");
            }
        }

        [HttpPost]
        public ActionResult CartDashboardAction(List<CartItemSession> cartItems, string actionType)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            if (actionType == "save")
            {
                // Call the service directly instead of redirecting
                service.UpdateCartItems(userId, cartItems);
                TempData["Success"] = "Cart updated successfully!";
                return RedirectToAction("CartDashboard");
            }

            if (actionType == "pay")
            {
                // Call your ProcessPayment action directly
                bool paymentSuccess = service.ProcessPayment(userId);
                if (paymentSuccess)
                {
                    TempData["Success"] = "Payment successful! Tickets generated.";
                    return RedirectToAction("MyTickets", "Dashboard");
                }
                else
                {
                    TempData["Error"] = "Payment failed. Please try again.";
                    return RedirectToAction("CartDashboard");
                }
            }

            return RedirectToAction("CartDashboard");
        }




    }
}
