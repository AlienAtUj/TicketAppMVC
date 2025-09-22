using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketAppMVC.Models
{
    public class EventReportDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public int TicketsRemaining { get; set; }
    }
}

