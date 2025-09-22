
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAppMVC.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string InvoiceNo { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public decimal TaxAmount { get; set; }

        public decimal Discount { get; set; }
        [Required]
        public decimal FinalPayment { get; set; }

        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }

        // Navigation
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
