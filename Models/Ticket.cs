using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAppMVC.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }  // FK to Event

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required]
        public int UserId { get; set; }  // FK to User (student)

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int Quantity { get; set; }   // How many tickets student bought

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now; // default to now
    }
}
