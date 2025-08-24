using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAppMVC.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(100)]
        public string Location { get; set; }

        [Required]
        public int OrganizerId { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual User Organizer { get; set; }

        [MaxLength(200)]
        public string EventImageUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

       
        [Required]
        public int TotalTickets { get; set; }

        [Required]
        public int SoldTickets { get; set; }
    }
}
