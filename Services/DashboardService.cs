using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using TicketAppMVC.Models;
using TicketAppMVC.Utils;


namespace TicketAppMVC.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext db;

        public DashboardService()
        {
            db = new ApplicationDbContext();
        }

       public List<Event> GetEventsForOrganizer(int UserId)
        {
            var list = from e in db.Events
                       where e.OrganizerId.Equals(UserId)
                       select e;
            return list.ToList();
        }

        public List<Event> GetAllEvents()
        {
            var query = from e in db.Events
                        select e;
            return query.ToList();
        }

        public List<Event> GetOrganizerEvents(int organizerId)
        {
            var query = from e in db.Events
                        where e.OrganizerId == organizerId
                        select e;
            return query.ToList();
        }

        public Event GetEventDetails(int eventId)
        {
            var query = from e in db.Events
                        where e.Id == eventId
                        select e;
            return query.FirstOrDefault();
        }


        public Cart GetOrCreateCart(int userId)
        {
            var query = from c in db.Carts
                        where c.UserId == userId && !c.IsDeleted
                        select c;

            Cart cart = query.FirstOrDefault();
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    IsDeleted = false
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            return cart;
        }

        public void AddToCart(int userId, int eventId, int? ticketTypeId, int quantity, decimal price)
        {
            Cart cart = GetOrCreateCart(userId);

            var query = from ci in db.CartItems
                        where ci.CartId == cart.Id && ci.EventId == eventId && ci.TicketTypeId == ticketTypeId && !ci.IsDeleted
                        select ci;

            CartItem cartItem = query.FirstOrDefault();

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.ModifiedAt = DateTime.Now;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    EventId = eventId,
                    TicketTypeId = ticketTypeId,
                    Quantity = quantity,
                    Price = price,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    IsDeleted = false
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();
        }

        public List<CartItemSession> GetCartSessionItems(int userId)
        {
       

            var cart = (from c in db.Carts
                        where c.UserId == userId && !c.IsDeleted
                        select c).FirstOrDefault();

            if (cart == null)
            {
                
                return new List<CartItemSession>();
            }

            var cartItems = (from ci in db.CartItems
                             where ci.CartId == cart.Id && !ci.IsDeleted
                             select new CartItemSession
                             {
                                 CartItemId = ci.Id,          // VERY IMPORTANT
                                 EventTitle = ci.Event.Title,
                                 Price = ci.Price,
                                 Quantity = ci.Quantity,
                                 EventDate = ci.Event.EventDate,
                                 Location = ci.Event.Location
                             }).ToList();


            return cartItems;
        }


        public void UpdateCartItems(int userId, List<CartItemSession> cartItems)
        {
            Console.WriteLine($"Updating cart items for UserId: {userId}");

            var cart = (from c in db.Carts
                        where c.UserId == userId && !c.IsDeleted
                        select c).FirstOrDefault();

            if (cart == null)
            {
                Console.WriteLine("No cart found for user. Nothing to update.");
                return;
            }

            foreach (var item in cartItems)
            {
                Console.WriteLine($"Received CartItem -> CartItemId: {item.CartItemId}, EventTitle: {item.EventTitle}, Quantity: {item.Quantity}, Price: {item.Price}");

                var dbItem = (from ci in db.CartItems
                              where ci.Id == item.CartItemId && ci.CartId == cart.Id && !ci.IsDeleted
                              select ci).FirstOrDefault();

                if (dbItem != null)
                {
                    Console.WriteLine($"Updating DB CartItemId: {dbItem.Id} from Quantity: {dbItem.Quantity} to Quantity: {item.Quantity}");
                    dbItem.Quantity = item.Quantity;
                    dbItem.ModifiedAt = DateTime.Now;
                }
                else
                {
                    Console.WriteLine($"CartItemId {item.CartItemId} not found in DB for this cart.");
                }
            }

            db.SaveChanges();
            Console.WriteLine("Cart update finished.");
        }





        public void DeleteCartItemByEventTitle(string eventTitle)
        {
            var eventQuery = from e in db.Events
                             where e.Title == eventTitle
                             select e.Id;

            int eventId = eventQuery.FirstOrDefault();

            var cartItemQuery = from ci in db.CartItems
                                where ci.EventId == eventId
                                select ci;

            CartItem cartItem = cartItemQuery.FirstOrDefault();
            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
            }
        }


        

        public bool DeleteCartItem(string eventTitle)
        {
            System.Diagnostics.Debug.WriteLine("DeleteCartItem called with eventTitle = " + eventTitle);

            // get eventId
            var eventId = (from e in db.Events
                           where e.Title.Equals(eventTitle)
                           select e.Id).FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("EventId found = " + eventId);

            // get cart item
            var cartItem = (from c in db.CartItems
                            where c.EventId.Equals(eventId)
                            select c).FirstOrDefault();

            if (cartItem != null)
            {
                
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
               
                return true;
            }

            System.Diagnostics.Debug.WriteLine("No CartItem found for eventId = " + eventId);
            return false;
        }



        public List<CartItem> GetCartItems(int userId)
        {
            var cartQuery = from c in db.Carts
                            where c.UserId == userId && !c.IsDeleted
                            select c;

            Cart cart = cartQuery.FirstOrDefault();
            if (cart == null) return new List<CartItem>();

            var query = from ci in db.CartItems
                        where ci.CartId == cart.Id && !ci.IsDeleted
                        select ci;

            return query.ToList();
        }



        public Event EventDetailIndex(int eventId)
        {
            // Get the event
            Event ev = db.Events.FirstOrDefault(e => e.Id == eventId);
            if (ev == null)
                return null;

            // Load organizer manually
            ev.Organizer = db.Users.FirstOrDefault(u => u.Id == ev.OrganizerId);
            if (ev.Organizer != null && string.IsNullOrEmpty(ev.Organizer.ProfileImageUrl))
                ev.Organizer.ProfileImageUrl = "/images/default-profile.jpg";

            return ev;
        }


        //Process Payment

        public bool ProcessPayment(int userId, string paymentMethod = "Student Account")
        {
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    Debug.WriteLine($"[ProcessPayment] Starting for UserId = {userId}");

                    // 1. Get user's cart
                    Debug.WriteLine($"[ProcessPayment] Step 1: Fetching cart for UserId = {userId}");
                    var cart = (from c in db.Carts
                                where c.UserId == userId && !c.IsDeleted
                                select c).FirstOrDefault();

                    if (cart == null)
                    {
                        Debug.WriteLine("[ProcessPayment] No active cart found.");
                        return false;
                    }

                    var cartItems = (from ci in db.CartItems
                                     where ci.CartId == cart.Id && !ci.IsDeleted
                                     select ci).ToList();

                    Debug.WriteLine($"[ProcessPayment] Found {cartItems.Count} items in cart.");

                    if (!cartItems.Any())
                    {
                        Debug.WriteLine("[ProcessPayment] Cart is empty.");
                        return false;
                    }

                    // 2. Calculate total amount
                    decimal totalAmount = cartItems.Sum(ci => ci.Price * ci.Quantity);
                    Debug.WriteLine($"[ProcessPayment] TotalAmount = {totalAmount}");

                    // 3. Create invoice with required InvoiceNo
                    var invoice = new Invoice
                    {
                        UserId = userId,
                        IssuedDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        TaxAmount = 0,
                        Discount = 0,
                        FinalPayment = totalAmount,
                        PaymentMethod = paymentMethod,
                        Status = "Completed",
                        InvoiceNo = new Random().Next(100000, 999999).ToString()
                    };

                    db.Invoices.Add(invoice);
                    db.SaveChanges(); // Save to get invoice.Id
                    Debug.WriteLine($"[ProcessPayment] Invoice created with Id = {invoice.Id}, InvoiceNo = {invoice.InvoiceNo}");

                    // 4. Create payment record
                    var payment = new Payment
                    {
                        InvoiceId = invoice.Id,
                        UserId = userId,
                        Amount = totalAmount,
                        PaymentDate = DateTime.Now,
                        PaymentStatus = "Completed",
                        TransactionId = "TXN_" + Guid.NewGuid().ToString("N").Substring(0, 12) // Safe length
                    };

                    db.Payments.Add(payment);
                    Debug.WriteLine("[ProcessPayment] Payment record created.");

                    // 5. Create tickets and update event availability
                    foreach (var cartItem in cartItems)
                    {
                        var eventRecord = (from e in db.Events
                                           where e.Id == cartItem.EventId
                                           select e).FirstOrDefault();

                        if (eventRecord != null)
                        {
                            if (eventRecord.AvailableTickets < cartItem.Quantity)
                            {
                                throw new Exception($"Not enough tickets available for {eventRecord.Title}");
                            }
                            eventRecord.AvailableTickets -= cartItem.Quantity;
                        }

                        for (int i = 0; i < cartItem.Quantity; i++)
                        {
                            string ticketCode = PaymentHelper.GenerateRandomTicketCode();
                            string qrFilename = $"{ticketCode}.png";
                            string qrPhysicalPath = System.Web.Hosting.HostingEnvironment.MapPath($"~/Content/Tickets/{qrFilename}");
                            string qrUrl = PaymentHelper.GenerateQRCode(ticketCode, qrPhysicalPath);

                            var ticket = new Ticket
                            {
                                EventId = cartItem.EventId,
                                UserId = userId,
                                TicketTypeId = cartItem.TicketTypeId,
                                TicketCode = ticketCode,
                                InvoiceId = invoice.Id,
                                BookingDate = DateTime.Now,
                                ModifiedAt = DateTime.Now,
                                Quantity = 1,
                                TotalPrice = cartItem.Price,
                                IsDeleted = false,
                                QRCodeUrl = qrUrl
                            };

                            db.Tickets.Add(ticket);
                        }
                    }
                    Debug.WriteLine("[ProcessPayment] Tickets created.");

                    // 6. Clear cart (soft delete)
                    cart.IsDeleted = true;
                    cart.ModifiedAt = DateTime.Now;

                    foreach (var cartItem in cartItems)
                    {
                        cartItem.IsDeleted = true;
                        cartItem.ModifiedAt = DateTime.Now;
                    }

                    // 7. Save all changes
                    db.SaveChanges();
                    Debug.WriteLine("[ProcessPayment] All changes saved successfully.");

                    // 8. Commit transaction
                    dbTransaction.Commit();
                    Debug.WriteLine("[ProcessPayment] Transaction committed successfully.");
                    return true;
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    dbTransaction.Rollback();

                    Debug.WriteLine("[ProcessPayment] Entity validation error(s):");
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        Debug.WriteLine($"-- Entity: {eve.Entry.Entity.GetType().Name}, State: {eve.Entry.State}");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Debug.WriteLine($"   Property: {ve.PropertyName}, Error: {ve.ErrorMessage}");
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    Debug.WriteLine($"[ProcessPayment] FAILED - Exception: {ex}");
                    return false;
                }
            }
        }



        public List<TicketViewModel> GetUserTickets(int userId)
        {
            var query = from t in db.Tickets
                        join e in db.Events on t.EventId equals e.Id
                        join u in db.Users on e.OrganizerId equals u.Id
                        where t.UserId == userId && !t.IsDeleted
                        orderby t.BookingDate descending
                        select new TicketViewModel
                        {
                            Id = t.Id,
                            EventTitle = e.Title,
                            EventDate = e.EventDate,
                            Location = e.Location,
                            TicketCode = t.TicketCode,
                            QRCodeUrl = t.QRCodeUrl,
                            Price = t.TotalPrice,
                            Quantity = t.Quantity,
                            BookingDate = t.BookingDate,
                            OrganizerName = u.Username
                        };

            return query.ToList();
        }

    }
}






