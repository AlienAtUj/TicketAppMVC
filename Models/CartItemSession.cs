using System;

namespace TicketAppMVC.Models
{
    public class CartItemSession
    {
        public int CartItemId { get; set; }

        // Event info
        public int EventId { get; set; }       // optional but good to keep
        public string EventTitle { get; set; }
        public DateTime? EventDate { get; set; }
        public string Location { get; set; }

        // Ticket info
        public int? TicketTypeId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal => Price * Quantity;
    }
}
