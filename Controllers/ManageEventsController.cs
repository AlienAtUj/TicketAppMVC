using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using TicketAppMVC.Models;
using TicketAppMVC.Services;

namespace TicketAppMVC.Controllers
{
    public class ManageEventsController : Controller
    {
        private readonly ManageEventService _service;

        public ManageEventsController()
        {
            _service = new ManageEventService(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

       
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int organizerId = Convert.ToInt32(Session["UserId"]);
            var events = _service.GetEventsByOrganizer(organizerId);
            return View(events);
        }

       
        public ActionResult EventManagement(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var ev = _service.GetEventById(id);
            if (ev == null)
                return HttpNotFound();

            return View(ev);
        }



        public ActionResult ViewAttendees(int? eventId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int organizerId = Convert.ToInt32(Session["UserId"]);

            var events = _service.GetEventsByOrganizer(organizerId);
            ViewBag.Events = events;
            ViewBag.SelectedEventId = eventId;

            if (!eventId.HasValue)
                return View(new List<Ticket>());

            var tickets = _service.GetTicketsByEvent(eventId.Value);
            return View(tickets);
        }

        [HttpPost]
        public ActionResult ViewAttendees(int eventDropdown)
        {
            return RedirectToAction("ViewAttendees", new { eventId = eventDropdown });
        }

       
        public ActionResult CreateEvent()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent(Event model, HttpPostedFileBase EventImage)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            model.OrganizerId = Convert.ToInt32(Session["UserId"]);
            _service.CreateEvent(model, EventImage);

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var ev = _service.GetEventById(id);
            if (ev == null) return HttpNotFound();
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event model)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateEvent(model);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _service.DeleteEvent(id);
            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult Reports()
        {
            var events = _service.GetTicketsSoldPerEvent();
            var users = _service.GetTicketsBoughtPerUser();

            return View("Reports", Tuple.Create(events, users));
        }




    }


}
