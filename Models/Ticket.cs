
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
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int? TicketTypeId { get; set; }

        [ForeignKey("TicketTypeId")]
        public virtual TicketType TicketType { get; set; }

        [Required, StringLength(50)]
        public string TicketCode { get; set; }

        public int? InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        public DateTime BookingDate { get; set; }
        public DateTime ModifiedAt { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public bool IsDeleted { get; set; }

        public string QRCodeUrl { get; set; }
    }
}
