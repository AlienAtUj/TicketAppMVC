using System;
namespace TicketAppMVC.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string TicketCode { get; set; }
        public string QRCodeUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime BookingDate { get; set; }
        public string OrganizerName { get; set; }
        public string TicketTypeName { get; set; } // Optional: if you want to show ticket type
    }
}