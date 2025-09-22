using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketAppMVC.Models
{
    public class ReportDto
    {
        public int TotalEventsWithSales { get; set; }
        public List<EventReportDto> EventSales { get; set; }
      
        public decimal TotalRevenue { get; set; }
        public EventReportDto TopEvent { get; set; }
    }

}
