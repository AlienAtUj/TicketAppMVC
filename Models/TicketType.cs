

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAppMVC.Models
{
    public class TicketType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required, StringLength(50)]
        public string TypeName { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Navigation
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
