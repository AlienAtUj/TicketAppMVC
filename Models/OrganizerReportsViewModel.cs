using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketAppMVC.Models
{
    public class OrganizerReportsViewModel
    {
        public List<EventReportDto> EventSales { get; set; }
        public List<UserReportDto> UserPurchases { get; set; }
    }
}