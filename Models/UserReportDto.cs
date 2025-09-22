using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketAppMVC.Models
{
    public class UserReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int TicketsBought { get; set; }
    }
}