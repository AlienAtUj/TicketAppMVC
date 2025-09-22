
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAppMVC.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [StringLength(150)]
        public string Location { get; set; }

        [Required]
        public int OrganizerId { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual User Organizer { get; set; }

        public string EventImageUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int TotalTickets { get; set; }

        [Required]
        public int AvailableTickets { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        public virtual ICollection<TicketType> TicketTypes { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<TicketType> Tickets { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
    }
}
