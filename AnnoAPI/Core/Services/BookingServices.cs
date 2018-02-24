using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Enum;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AnnoAPI.Core.Services
{
    public class BookingServices
    {
        MySqlUtility databaseAnno = null;

        public BookingServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }
        
        public BookEventResponse BookEvent(long hostId, BookEventRequest value, out string status)
        {
            BookEventResponse result = null;

            //Validate if customer exist
            CustomerServices customerServices = new CustomerServices();
            var customer = customerServices.GetCustomerByRef(hostId, value.CustomerReferenceId);
            if (customer == null)
            {
                status = BookEventStatuses.CustomerNotFound;
                return null;
            }

            //Validate if event and tier exist
            EventsServices eventsServices = new EventsServices();
            Event eventInfo = eventsServices.GetEventByRef(hostId, value.EventReferenceId);
            if(eventInfo == null)
            {
                status = BookEventStatuses.EventNotFound;
                return null;
            }
            List<EventTier> eventTiers = eventsServices.GetEventTiersByEventId(eventInfo.EventId.Value);
            if (eventTiers == null && eventTiers.Count == 0)
            {
                status = BookEventStatuses.EventNotFound;
                return null;
            }

            //Validate event status
            if (eventInfo.Status != "Active")
            {
                status = BookEventStatuses.EventNotActive;
                return null;
            }

            //Validate if event has already started
            if (DateTime.UtcNow >= eventInfo.StartDate)
            {
                status = BookEventStatuses.EventHasAlreadyStarted;
                return null;
            }

            //Validate if all requested tiers exist
            List<string> allAvailableEventTiers = eventTiers.Select(x => x.ReferenceId).ToList();
            List<string> requestedEventTiers = value.Tickets.Select(x => x.EventTierReferenceId).ToList();
            bool contained = !requestedEventTiers.Except(allAvailableEventTiers).Any();
            if(!contained)
            {
                status = BookEventStatuses.TierNotFound;
                return null;
            }

            //Validate requested ticket quantities (must be more than zero)
            if(value.Tickets.Where(x => x.Quantity < 1).Count() > 0)
            {
                status = BookEventStatuses.InvalidTicketQuantity;
                return null;
            }
            
            //Validate if tickets are still available for the requested quantities
            foreach (var ticket in value.Tickets)
            {
                var tier = eventTiers.Where(x => x.ReferenceId == ticket.EventTierReferenceId).ToList()[0];
                if (tier.AvailableTickets < ticket.Quantity)
                {
                    status = BookEventStatuses.InsufficientTickets;
                    return null;
                }
            }
            
            //Calculate total cost
            long totalCost = 0;
            foreach(var ticket in value.Tickets)
            {
                var tier = eventTiers.Where(x => x.ReferenceId == ticket.EventTierReferenceId).ToList()[0];
                totalCost += (ticket.Quantity * tier.Price.Value);
            }

            //Check customer wallet balance
            if(customer.WalletBalance < totalCost)
            {
                status = BookEventStatuses.CustomerInsufficientFunds;
                return null;
            }

            //TODO: Transaction lock for thread safety

            string bookingConfirmation = null;
            string ticketNumber = null;
            string ticketAddress = null;
            List<KeyValuePair<long, string>> tierTicketNumbers = new List<KeyValuePair<long, string>>();
            List<KeyValuePair<long, string>> tierTicketAddresses = new List<KeyValuePair<long, string>>();

            //Insert customer booking
            bookingConfirmation = GenerateBookingConfirmation(hostId);
            long bookingId = InsertUserBooking(customer.CustomerId.Value, eventInfo.EventId.Value, bookingConfirmation);

            //Perform transaction
            WalletServices walletService = new WalletServices();
            walletService.Transfer(customer.WalletAddress, eventInfo.WalletAddress, totalCost, bookingId, "Booking");

            //Insert customer tickets
            foreach (var ticketPurchase in value.Tickets)
            {
                var tier = eventTiers.Where(x => x.ReferenceId == ticketPurchase.EventTierReferenceId).ToList()[0];

                for (int i = 0; i < ticketPurchase.Quantity; i++)
                {
                    ticketNumber = GenerateTicketNumber(eventInfo.EventId.Value);
                    ticketAddress = HashUtility.GenerateHash();

                    InsertUserTicket(customer.CustomerId.Value, bookingId, eventInfo.EventId.Value, tier.TierId.Value, ticketNumber, tier.Price.Value, "Active", ticketAddress);

                    tierTicketNumbers.Add(new KeyValuePair<long, string>(tier.TierId.Value, ticketNumber));
                    tierTicketAddresses.Add(new KeyValuePair<long, string>(tier.TierId.Value, ticketAddress));
                }
            }

            //Update tickets availability
            foreach (var ticket in value.Tickets)
            {
                var tier = eventTiers.Where(x => x.ReferenceId == ticket.EventTierReferenceId).ToList()[0];
                UpdateAvailableTickets(tier.TierId.Value, tier.AvailableTickets.Value - ticket.Quantity);
            }

            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            for (int i = 0; i < value.Tickets.Count; i++)
            {
                var ticket = value.Tickets[i];
                var tier = eventTiers
                    .Where(x => x.ReferenceId == ticket.EventTierReferenceId).ToList()[0];

                List<string> ticketNumbers = tierTicketNumbers
                    .Where(x => x.Key == tier.TierId.Value)
                    .Select(x => x.Value).ToList();

                List<string> ticketAddresses = tierTicketAddresses
                    .Where(x => x.Key == tier.TierId.Value)
                    .Select(x => x.Value).ToList();

                blockchainContract.BookEventTier(customer.CustomerAddress, eventInfo.EventAddress, tier.TierAddress, ticket.Quantity, ticketNumbers, ticketAddresses);
            }

            result = new BookEventResponse();
            result.ConfirmationNumber = bookingConfirmation;
            result.TicketNumbers = tierTicketNumbers.Select(x => x.Value).ToList();

            status = BookEventStatuses.Success;
            return result;
        }

        public List<Ticket> GetTicketsByEvent(long hostId, string eventReferenceId)
        {
            List<Ticket> result = new List<Ticket>();

            string sql = @"SELECT tic.ticket_id, tic.ticket_number, tic.seat_number, tic.status, tic.paid_price,
                                b.confirmation_number, e.ref_id as event_ref, u.ref_id as customer_ref, t.ref_id as tier_ref 
                                FROM customer_ticket tic
                                INNER JOIN customer_booking b ON (b.booking_id = tic.booking_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                INNER JOIN events_tier t ON (t.event_id = e.event_id AND tic.tier_id = t.tier_id)
                                INNER JOIN customer u ON (u.customer_id = b.customer_id)
                                WHERE b.record_status='Live'
                                AND e.record_status='Live'
                                AND tic.record_status='Live'
                                AND e.host_id=@hostId  
                                AND e.ref_id=@eventReferenceId";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@eventReferenceId", DataUtility.ToMySqlParam(eventReferenceId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                foreach(DataRow row in dt.Rows)
                {
                    result.Add(new Ticket()
                    {
                        TicketId = ConvertUtility.ToInt64(row["ticket_id"]),
                        CustomerReferenceId = ConvertUtility.ToString(row["customer_ref"]),
                        EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                        TierReferenceId = ConvertUtility.ToString(row["tier_ref"]),
                        TicketNo = ConvertUtility.ToString(row["ticket_number"]),
                        BookingConfirmationNo = ConvertUtility.ToString(row["confirmation_number"]),
                        Status = ConvertUtility.ToString(row["status"]),
                        PaidPrice = ConvertUtility.ToInt64(row["paid_price"])
                    });
                }
            }

            return result;
        }

        public List<Booking> GetBookingsByEvent(long hostId, string eventReferenceId)
        {
            List<Booking> result = null;

            string sql = @"SELECT b.confirmation_number, u.ref_id as customer_ref, e.ref_id as event_ref, e.title
                                FROM customer_booking b
                                INNER JOIN customer u ON (u.customer_id = b.customer_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                WHERE b.record_status='Live'
                                AND e.record_status='Live'
                                AND e.host_id=@hostId  
                                AND e.ref_id=@eventReferenceId";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@eventReferenceId", DataUtility.ToMySqlParam(eventReferenceId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<Booking>();
                
                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new Booking()
                    {
                        UserReferenceId = ConvertUtility.ToString(row["customer_ref"]),
                        EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                        EventTitle = ConvertUtility.ToString(row["title"]),
                        ConfirmationNumber = ConvertUtility.ToString(row["confirmation_number"])
                    });
                }
            }

            return result;
        }

        public List<Booking> GetBookingsByCustomer(long hostId, string customerReferenceId)
        {
            List<Booking> result = null;

            string sql = @"SELECT b.confirmation_number, u.ref_id as customer_ref, e.ref_id as event_ref, e.title
                                FROM customer_booking b
                                INNER JOIN customer u ON (u.customer_id = b.customer_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                WHERE b.record_status='Live'
                                AND e.record_status='Live'
                                AND e.host_id=@hostId  
                                AND u.ref_id=@customerReferenceId";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@customerReferenceId", DataUtility.ToMySqlParam(customerReferenceId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<Booking>();

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new Booking()
                    {
                        UserReferenceId = ConvertUtility.ToString(row["customer_ref"]),
                        EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                        EventTitle = ConvertUtility.ToString(row["title"]),
                        ConfirmationNumber = ConvertUtility.ToString(row["confirmation_number"])
                    });
                }
            }

            return result;
        }

        public BookingDetails GetBookingByConfirmationNo(long hostId, string confirmationNo)
        {
            BookingDetails result = null;

            string sql = @"SELECT b.confirmation_number, e.title as event_title, t.title as tier_title, e.ref_id as event_ref, t.ref_id as tier_ref, u.ref_id as customer_ref, 
                                tic.ticket_id, tic.ticket_number, tic.status as ticket_status, tic.paid_price, tic.address as ticket_address
                                FROM customer_booking b
                                INNER JOIN customer_ticket tic ON (tic.customer_id = b.customer_id AND tic.event_id = b.event_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                INNER JOIN events_tier t ON (t.tier_id = tic.tier_id)
                                INNER JOIN customer u ON (u.customer_id = tic.customer_id)
                                WHERE b.record_status='Live'
                                AND tic.record_status='Live'
                                AND e.record_status='Live'
                                AND t.record_status='Live'
                                AND e.host_id=@hostId  
                                AND b.confirmation_number=@confirmationNo";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@confirmationNo", DataUtility.ToMySqlParam(confirmationNo));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow firstRow = dt.Rows[0];

                result = new BookingDetails();
                result.UserReferenceId = ConvertUtility.ToString(firstRow["customer_ref"]);
                result.EventReferenceId = ConvertUtility.ToString(firstRow["event_ref"]);
                result.ConfirmationNumber = ConvertUtility.ToString(firstRow["confirmation_number"]);
                result.Tickets = new List<Ticket>();

                foreach (DataRow row in dt.Rows)
                {
                    Ticket ticket = new Ticket();
                    ticket.TicketId = ConvertUtility.ToInt64(row["ticket_id"]);
                    ticket.CustomerReferenceId = ConvertUtility.ToString(row["customer_ref"]);
                    ticket.EventReferenceId = ConvertUtility.ToString(row["event_ref"]);
                    ticket.TierReferenceId = ConvertUtility.ToString(row["tier_ref"]);
                    ticket.TicketNo = ConvertUtility.ToString(row["ticket_number"]);
                    ticket.Status = ConvertUtility.ToString(row["ticket_status"]);
                    ticket.PaidPrice = ConvertUtility.ToInt64(row["paid_price"]);
                    ticket.TicketAddress = ConvertUtility.ToString(row["ticket_address"]);
                    ticket.EventTitle = ConvertUtility.ToString(row["event_title"]);
                    ticket.TierTitle = ConvertUtility.ToString(row["tier_title"]);
                    ticket.BookingConfirmationNo = ConvertUtility.ToString(row["confirmation_number"]);
                    result.Tickets.Add(ticket);
                }
            }

            return result;
        }

        public List<BookingSummary> GetBookingsSummary(long hostId)
        {
            List<BookingSummary> result = null;

            string sql = @"SELECT e.ref_id as event_ref, e.title as event_title, t.ref_id as tier_ref, t.title as tier_title, count(tic.ticket_id) as tickets_sold
                                FROM customer_booking b
                                INNER JOIN customer_ticket tic ON (tic.booking_id = b.booking_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                INNER JOIN events_tier t ON (t.event_id = e.event_id and t.tier_id=tic.tier_id)
                                WHERE b.record_status='Live'
                                AND e.record_status='Live'
                                AND e.host_id=@hostId 
                                GROUP BY e.event_id, t.tier_id";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<BookingSummary>();
                
                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new BookingSummary()
                    {
                        EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                        TierReferenceId = ConvertUtility.ToString(row["tier_ref"]),
                        EventTitle = ConvertUtility.ToString(row["event_title"]),
                        TierTitle = ConvertUtility.ToString(row["tier_title"]),
                        TicketsSoldCount = ConvertUtility.ToInt32(row["tickets_sold"])
                    });
                }
            }

            return result;
        }

        public long InsertUserBooking(long customerId, long eventId, string confirmationNumber)
        {
            long newBookingId = 0;

            //Insert customer booking to database
            string sql = @"INSERT INTO customer_booking (customer_id, event_id, confirmation_number, record_status, created_date) 
                            VALUES (@customer_id, @event_id, @confirmation_number, @record_status, @created_date)";

            sql = sql.Replace("@customer_id", DataUtility.ToMySqlParam(customerId))
                    .Replace("@event_id", DataUtility.ToMySqlParam(eventId))
                    .Replace("@confirmation_number", DataUtility.ToMySqlParam(confirmationNumber))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            this.databaseAnno.Execute(sql, out newBookingId);

            return newBookingId;
        }

        public long InsertUserTicket(long customerId, long bookingId, long eventId, long tierId, string ticketNumber, decimal paidPrice, string status, string address)
        {
            long newTicketId = 0;

            //Insert customer ticket to database
            string sql = @"INSERT INTO customer_ticket (customer_id, booking_id, event_id, tier_id, ticket_number, status, seat_number, paid_price, address, record_status, created_date) 
                            VALUES (@customer_id, @booking_id, @event_id, @tier_id, @ticket_no, @status, @seat_number, @paid_price, @address, @record_status, @created_date)";

            sql = sql.Replace("@customer_id", DataUtility.ToMySqlParam(customerId))
                    .Replace("@booking_id", DataUtility.ToMySqlParam(bookingId))
                    .Replace("@event_id", DataUtility.ToMySqlParam(eventId))
                    .Replace("@tier_id", DataUtility.ToMySqlParam(tierId))
                    .Replace("@ticket_no", DataUtility.ToMySqlParam(ticketNumber))
                    .Replace("@status", DataUtility.ToMySqlParam(status))
                    .Replace("@seat_number", DataUtility.ToMySqlParam(null))
                    .Replace("@paid_price", DataUtility.ToMySqlParam(paidPrice))
                    .Replace("@address", DataUtility.ToMySqlParam(address))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            this.databaseAnno.Execute(sql, out newTicketId);

            return newTicketId;
        }

        public void UpdateAvailableTickets(long tierId, int availableTickets)
        {
            string sql = @"UPDATE events_tier SET available_tickets=@availableTickets WHERE tier_id=@tierId";

            sql = sql.Replace("@tierId", DataUtility.ToMySqlParam(tierId))
                    .Replace("@availableTickets", DataUtility.ToMySqlParam(availableTickets));

            this.databaseAnno.Execute(sql);
        }

        #region Private Methods

        /// <summary>
        /// Generates a new booking confirmation:
        /// E{HostId}{RandomAlphanumeric8Length}
        /// </summary>
        private string GenerateBookingConfirmation(long hostId)
        {
            //TODO: regenerate number if duplicate found in database
            return string.Format("E{0}{1}", hostId, TextUtility.RandomAlphanumeric(8).ToUpper());
        }

        /// <summary>
        /// Generates a new ticket number:
        /// T{EventId}-{RandomAlphanumeric8Length}
        /// </summary>
        private string GenerateTicketNumber(long eventId)
        {
            //TODO: regenerate number if duplicate found in database
            return string.Format("T{0}{1}", eventId, TextUtility.RandomAlphanumeric(8).ToUpper());
        }

        #endregion

    }
}