using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketAppMVC.Models
{
    public class TicketSalesReportDto
    {
        public string EventTitle { get; set; }
        public int TotalTicketsSold { get; set; }
        public int UniqueUsers { get; set; }
        public decimal TotalRevenue { get; set; }
    }

}