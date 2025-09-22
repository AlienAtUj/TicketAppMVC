using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using TicketAppMVC.Models;

namespace TicketAppMVC.Services
{
    public class ManageEventService
    {
        private readonly string _connStr;
        private readonly ApplicationDbContext db;

        public ManageEventService(string connectionString)
        {
            _connStr = connectionString;
            db = new ApplicationDbContext();
        }


        public List<Event> GetEventsByOrganizer(int organizerId)
        {
            List<Event> events = new List<Event>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = "SELECT * FROM Events WHERE OrganizerId=@OrganizerId ORDER BY EventDate DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrganizerId", organizerId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    events.Add(MapEvent(row));
                }
            }

            return events;
        }

       
        public Event GetEventById(int eventId)
        {
            Event ev = null;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = "SELECT * FROM Events WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", eventId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    ev = MapEvent(dt.Rows[0]);
                }
            }
            return ev;
        }


        public void CreateEvent(Event model, HttpPostedFileBase EventImage)
        {
            // Handle Event Image
            if (EventImage != null && EventImage.ContentLength > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(EventImage.FileName);
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/EventImages"), fileName);
                EventImage.SaveAs(path);
                model.EventImageUrl = "/Content/EventImages/" + fileName;
            }
            else
            {
                model.EventImageUrl = "/Content/EventImages/default-event.jpg";
            }

            // Initialize ticket counts
            model.CreatedAt = DateTime.Now;
            model.ModifiedAt = DateTime.Now;
            model.AvailableTickets = model.TotalTickets;   // All tickets are available

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = @"
            INSERT INTO Events 
            (Title, Description, EventDate, Location, OrganizerId, EventImageUrl, Price, TotalTickets, AvailableTickets, CreatedAt, ModifiedAt)
            VALUES
            (@Title, @Description, @EventDate, @Location, @OrganizerId, @EventImageUrl, @Price, @TotalTickets, @AvailableTickets, @CreatedAt, @ModifiedAt)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
                cmd.Parameters.AddWithValue("@EventDate", model.EventDate);
                cmd.Parameters.AddWithValue("@Location", model.Location ?? "");
                cmd.Parameters.AddWithValue("@OrganizerId", model.OrganizerId);
                cmd.Parameters.AddWithValue("@EventImageUrl", model.EventImageUrl);
                cmd.Parameters.AddWithValue("@Price", model.Price);
                cmd.Parameters.AddWithValue("@TotalTickets", model.TotalTickets);
                cmd.Parameters.AddWithValue("@AvailableTickets", model.AvailableTickets);
                cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt);
                cmd.Parameters.AddWithValue("@ModifiedAt", model.ModifiedAt);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public List<Ticket> GetTicketsByEvent(int eventId)
        {
            List<Ticket> tickets = new List<Ticket>();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = @"
                    SELECT t.*, u.Username, u.Email
                    FROM Tickets t
                    JOIN Users u ON t.UserId = u.Id
                    WHERE t.EventId=@EventId AND t.IsDeleted=0
                    ORDER BY t.BookingDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EventId", eventId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var ticket = new Ticket
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        EventId = Convert.ToInt32(row["EventId"]),
                        UserId = Convert.ToInt32(row["UserId"]),
                        TicketTypeId = row["TicketTypeId"] != DBNull.Value ? Convert.ToInt32(row["TicketTypeId"]) : (int?)null,
                        TicketCode = row["TicketCode"].ToString(),
                        InvoiceId = row["InvoiceId"] != DBNull.Value ? Convert.ToInt32(row["InvoiceId"]) : (int?)null,
                        BookingDate = Convert.ToDateTime(row["BookingDate"]),
                        ModifiedAt = Convert.ToDateTime(row["ModifiedAt"]),
                        Quantity = Convert.ToInt32(row["Quantity"]),
                        TotalPrice = Convert.ToDecimal(row["TotalPrice"]),
                        IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                        User = new User
                        {
                            Id = Convert.ToInt32(row["UserId"]),
                            Username = row["Username"].ToString(),
                            Email = row["Email"].ToString()
                        }
                    };

                    tickets.Add(ticket);
                }
            }
            return tickets;
        }

      
        private Event MapEvent(DataRow row)
        {
            return new Event
            {
                Id = Convert.ToInt32(row["Id"]),
                Title = row["Title"].ToString(),
                Description = row["Description"].ToString(),
                EventDate = Convert.ToDateTime(row["EventDate"]),
                Location = row["Location"].ToString(),
                OrganizerId = Convert.ToInt32(row["OrganizerId"]),
                EventImageUrl = row["EventImageUrl"] != DBNull.Value ? row["EventImageUrl"].ToString() : "/Content/EventImages/default-event.jpg",
                Price = Convert.ToDecimal(row["Price"]),
                TotalTickets = Convert.ToInt32(row["TotalTickets"]),
                AvailableTickets = Convert.ToInt32(row["AvailableTickets"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                ModifiedAt = Convert.ToDateTime(row["ModifiedAt"])
            };
        }
        public void UpdateEvent(Event model)
        {
            model.ModifiedAt = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = @"
        UPDATE Events
        SET Title=@Title,
            Description=@Description,
            EventDate=@EventDate,
            Location=@Location,
            Price=@Price,
            TotalTickets=@TotalTickets,
            AvailableTickets = @TotalTickets - SoldTickets,  -- recalc available tickets
            ModifiedAt=@ModifiedAt
        WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
                cmd.Parameters.AddWithValue("@EventDate", model.EventDate);
                cmd.Parameters.AddWithValue("@Location", model.Location ?? "");
                cmd.Parameters.AddWithValue("@Price", model.Price);
                cmd.Parameters.AddWithValue("@TotalTickets", model.TotalTickets);
                cmd.Parameters.AddWithValue("@ModifiedAt", model.ModifiedAt);
                cmd.Parameters.AddWithValue("@Id", model.Id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteEvent(int eventId)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                string query = "DELETE FROM Events WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", eventId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<EventReportDto> GetTicketsSoldPerEvent()
        {
            var query = db.Events
                .Select(e => new EventReportDto
                {
                    EventId = e.Id,
                    EventName = e.Title,
                    TotalTickets = e.TotalTickets,
                    SoldTickets = e.TotalTickets - e.AvailableTickets,
                    TicketsRemaining = e.AvailableTickets 
                })
                .ToList();

            return query;
        }

        public List<UserReportDto> GetTicketsBoughtPerUser()
        {
            var query = db.Users
                .Select(u => new UserReportDto
                {
                    UserId = u.Id,
                    UserName = u.Username,
                    TicketsBought = db.Tickets
                        .Where(t => t.UserId == u.Id)
                        .Sum(t => (int?)t.Quantity) ?? 0
                })
                .ToList();

            return query;
        }



    }

}
