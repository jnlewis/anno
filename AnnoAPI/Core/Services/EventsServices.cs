using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Enum;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace AnnoAPI.Core.Services
{
    public class EventsServices
    {
        MySqlUtility databaseAnno = null;

        public EventsServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }

        /// <summary>
        /// Inserts a new event record in the Events table.
        /// </summary>
        public void CreateEvent(long hostId, CreateEventsRequest value)
        {
            long newEventId = 0;
            long newTierId = 0;
            List<string> tierAddresses = new List<string>();

            //Generate address
            string eventAddress = HashUtility.GenerateHash();

            //Insert event to database
            string sql = @"INSERT INTO events (host_id, ref_id, title, description, start_date, status, address, record_status, created_date) 
                            VALUES (@host_id, @ref_id, @title, @description, @start_date, @status, @address, @record_status, @created_date)";

            sql = sql.Replace("@host_id", DataUtility.ToMySqlParam(hostId))
                    .Replace("@ref_id", DataUtility.ToMySqlParam(value.ReferenceId))
                    .Replace("@title", DataUtility.ToMySqlParam(value.Title))
                    .Replace("@description", DataUtility.ToMySqlParam(value.Description))
                    .Replace("@start_date", DataUtility.ToMySqlParam(value.StartDate))
                    .Replace("@status", DataUtility.ToMySqlParam("Active"))
                    .Replace("@address", DataUtility.ToMySqlParam(eventAddress))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Pending))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            this.databaseAnno.Execute(sql, out newEventId);

            //Insert event tiers to database
            foreach (var tier in value.Tiers)
            {
                //Generate address
                string tierAddress = HashUtility.GenerateHash();

                sql = @"INSERT INTO events_tier (host_id, event_id, ref_id, title, description, total_tickets, available_tickets, price, status, address, record_status, created_date) 
                            VALUES (@host_id, @event_id, @ref_id, @title, @description, @total_tickets, @available_tickets, @price, @status, @address, @record_status, @created_date)";

                sql = sql.Replace("@host_id", DataUtility.ToMySqlParam(hostId))
                        .Replace("@event_id", DataUtility.ToMySqlParam(newEventId))
                        .Replace("@ref_id", DataUtility.ToMySqlParam(tier.ReferenceId))
                        .Replace("@title", DataUtility.ToMySqlParam(tier.Title))
                        .Replace("@description", DataUtility.ToMySqlParam(tier.Description))
                        .Replace("@total_tickets", DataUtility.ToMySqlParam(tier.TotalTickets))
                        .Replace("@available_tickets", DataUtility.ToMySqlParam(tier.TotalTickets)) //available tickets is set to total tickets for initial insert
                        .Replace("@price", DataUtility.ToMySqlParam(tier.Price))
                        .Replace("@status", DataUtility.ToMySqlParam("Active"))
                        .Replace("@address", DataUtility.ToMySqlParam(tierAddress))
                        .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                        .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

                this.databaseAnno.Execute(sql, out newTierId);

                tierAddresses.Add(tierAddress);
            }

            //Update event record status to live
            sql = @"UPDATE events SET record_status=@record_status WHERE event_id=@event_id";
            sql = sql.Replace("@event_id", DataUtility.ToMySqlParam(newEventId))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live));

            this.databaseAnno.Execute(sql);

            //Create event wallet
            WalletServices walletServices = new WalletServices();
            walletServices.CreateWallet(newEventId, WalletOwnerTypes.Event, eventAddress);

            //Commit to blockchain
            IdentityServices identityServices = new IdentityServices();
            string hostAddress = identityServices.AddressOf(IdentityServices.AddressTypes.Host, hostId);

            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.CreateEvent(eventAddress, hostAddress, value.ReferenceId, value.Title, value.StartDate.Value, "Active");

            for(int i=0; i<value.Tiers.Count; i++)
            {
                blockchainContract.CreateEventTier(
                    tierAddresses[i], hostAddress, eventAddress, value.Tiers[i].ReferenceId, value.Tiers[i].Title,
                    value.Tiers[i].TotalTickets.Value, value.Tiers[i].TotalTickets.Value, value.Tiers[i].Price.Value);
            }
        }

        public void UpdateEventStatus(long hostId, UpdateEventStatusRequest value)
        {
            //update database
            string sql = @"UPDATE events SET status=@status WHERE ref_id=@ref_id AND record_status=@record_status";

            sql = sql.Replace("@status", DataUtility.ToMySqlParam(value.NewStatus))
                    .Replace("@ref_id", DataUtility.ToMySqlParam(value.ReferenceId))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live));

            this.databaseAnno.Execute(sql);
        }

        public string CancelEvent(long hostId, CancelEventRequest value)
        {
            //Validate if event
            EventsServices eventsServices = new EventsServices();
            Event eventInfo = eventsServices.GetEventByRef(hostId, value.ReferenceId);
            if (eventInfo == null)
            {
                return CancelEventStatuses.EventNotFound;
            }

            //Validate event status
            if (eventInfo.Status != EventStatuses.Active)
            {
                return CancelEventStatuses.EventNotActive;
            }

            //Validate if event start date is over
            if (DateTime.UtcNow >= eventInfo.StartDate)
            {
                return CancelEventStatuses.EventHasAlreadyStarted;
            }

            //Cancel and Refund all tickets
            BookingServices bookingService = new BookingServices();
            var tickets = bookingService.GetTicketsByEvent(hostId, value.ReferenceId);
            if(tickets != null)
            {
                foreach (var ticket in tickets)
                {
                    string status = null;
                    CancelAndRefundTicket(hostId, ticket.TicketId.Value, out status);
                    //Not required to check on status as cancel event will try to cancel and refund all tickets
                }
            }

            //Update event status to Cancelled
            UpdateEventStatusRequest req = new UpdateEventStatusRequest();
            req.ReferenceId = value.ReferenceId;
            req.NewStatus = EventStatuses.Cancelled;
            UpdateEventStatus(hostId, req);

            return CancelEventStatuses.Success;
        }
        
        public ClaimEarningsResponse ClaimEarnings(long hostId, ClaimEarningsRequest value, out string status)
        {
            ClaimEarningsResponse result = null;

            //Get event
            var eventInfo = GetEventByRef(hostId, value.ReferenceId);
            if(eventInfo == null)
            {
                status = ClaimEarningsStatuses.EventNotFound;
                return null;
            }

            //Check event status
            if(eventInfo.Status == EventStatuses.Cancelled)
            {
                status = ClaimEarningsStatuses.EventAlreadyCancelled;
                return null;
            }
            if (eventInfo.Status == EventStatuses.Closed)
            {
                status = ClaimEarningsStatuses.EventAlreadyClaimed;
                return null;
            }
            
            //Check if event has already started
            if (DateTime.UtcNow < eventInfo.StartDate.Value)
            {
                status = ClaimEarningsStatuses.EventNotStarted;
                return null;
            }
            
            //Get host
            HostServices hostServices = new HostServices();
            var host = hostServices.GetHostInfoById(hostId);
            if(host == null)
            {
                status = ClaimEarningsStatuses.HostNotFound;
                return null;
            }

            //Transfer event wallet balance to host wallet
            WalletServices walletServices = new WalletServices();
            walletServices.Transfer(eventInfo.WalletAddress, host.WalletAddress, eventInfo.WalletBalance.Value, null, "Claim");

            //Update event status
            UpdateEventStatusRequest req = new UpdateEventStatusRequest();
            req.ReferenceId = eventInfo.ReferenceId;
            req.NewStatus = EventStatuses.Closed;
            UpdateEventStatus(hostId, req);

            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.ClaimEarnings(eventInfo.EventAddress);

            status = ClaimEarningsStatuses.Success;
            
            result = new ClaimEarningsResponse();
            result.EventTitle = eventInfo.Title;
            result.Earnings = eventInfo.WalletBalance.Value;

            return result;
        }

        public List<Event> GetEvents(long hostId)
        {
            List<Event> result = null;
            
            string sql = @"SELECT e.event_id, e.ref_id, e.title, e.description, e.status, e.start_date, e.address as event_address, w.address, w.balance
                                FROM events e
                                INNER JOIN wallet w ON (w.owner_id=e.event_id AND w.owner_type=@ownerType)
                                WHERE e.record_status=@recordStatus
                                AND w.record_status=@recordStatus
                                AND e.host_id=@hostId ";

            sql = sql.Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Event))
                    .Replace("@hostId ", DataUtility.ToMySqlParam(hostId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<Event>();

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new Event()
                    {
                        EventId = ConvertUtility.ToInt64(row["event_id"]),
                        ReferenceId = ConvertUtility.ToString(row["ref_id"]),
                        Title = ConvertUtility.ToString(row["title"]),
                        Description = ConvertUtility.ToString(row["description"]),
                        Status = ConvertUtility.ToString(row["status"]),
                        StartDate = ConvertUtility.ToDateTime(row["start_date"]),
                        EventAddress = ConvertUtility.ToString(row["event_address"]),
                        WalletAddress = ConvertUtility.ToString(row["address"]),
                        WalletBalance = ConvertUtility.ToInt64(row["balance"])
                    });
                }
            }

            return result;
        }

        public Event GetEventByRef(long hostId, string refId)
        {
            Event result = null;

            string sql = @"SELECT e.event_id, e.ref_id, e.title, e.description, e.status, e.start_date, e.address as event_address, w.address, w.balance
                                FROM events e
                                INNER JOIN wallet w ON (w.owner_id=e.event_id AND w.owner_type=@ownerType)
                                WHERE e.record_status=@recordStatus
                                AND w.record_status=@recordStatus
                                AND e.host_id=@hostId 
                                AND e.ref_id=@refId";

            sql = sql
                .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Event))
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@refId", DataUtility.ToMySqlParam(refId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new Event()
                {
                    EventId = ConvertUtility.ToInt64(row["event_id"]),
                    ReferenceId = ConvertUtility.ToString(row["ref_id"]),
                    Title = ConvertUtility.ToString(row["title"]),
                    Description = ConvertUtility.ToString(row["description"]),
                    Status = ConvertUtility.ToString(row["status"]),
                    StartDate = ConvertUtility.ToDateTime(row["start_date"]),
                    EventAddress = ConvertUtility.ToString(row["event_address"]),
                    WalletAddress = ConvertUtility.ToString(row["address"]),
                    WalletBalance = ConvertUtility.ToInt64(row["balance"])
                };
            }

            return result;
        }

        public List<EventTier> GetEventTiersByEventId(long eventId)
        {
            List<EventTier> result = null;
            
            string sql = @"SELECT tier_id, event_id, ref_id, title, description, total_tickets, available_tickets, price, status, address as tier_address
                                FROM events_tier
                                WHERE record_status=@recordStatus
                                AND event_id=@eventId";

            sql = sql
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                .Replace("@eventId", DataUtility.ToMySqlParam(eventId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<EventTier>();

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new EventTier()
                    {
                        TierId = ConvertUtility.ToInt64(row["tier_id"]),
                        EventId = ConvertUtility.ToInt64(row["event_id"]),
                        ReferenceId = ConvertUtility.ToString(row["ref_id"]),
                        Title = ConvertUtility.ToString(row["title"]),
                        Description = ConvertUtility.ToString(row["description"]),
                        TotalTickets = ConvertUtility.ToInt32(row["total_tickets"]),
                        AvailableTickets = ConvertUtility.ToInt32(row["available_tickets"]),
                        Price = ConvertUtility.ToInt64(row["price"]),
                        Status = ConvertUtility.ToString(row["status"]),
                        TierAddress = ConvertUtility.ToString(row["tier_address"])
                    });
                }
            }

            return result;
        }

        public EventTier GetEventTiersByRef(long hostId, string refId)
        {
            EventTier result = null;

            string sql = @"SELECT tier_id, event_id, ref_id, title, description, total_tickets, available_tickets, price, status
                                FROM events_tier
                                WHERE record_status=@recordStatus
                                AND host_id=@hostId
                                AND ref_id=@refId";

            sql = sql
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                .Replace("@refId", DataUtility.ToMySqlParam(refId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new EventTier()
                {
                    TierId = ConvertUtility.ToInt64(row["tier_id"]),
                    EventId = ConvertUtility.ToInt64(row["event_id"]),
                    ReferenceId = ConvertUtility.ToString(row["ref_id"]),
                    Title = ConvertUtility.ToString(row["title"]),
                    Description = ConvertUtility.ToString(row["description"]),
                    TotalTickets = ConvertUtility.ToInt32(row["total_tickets"]),
                    AvailableTickets = ConvertUtility.ToInt32(row["available_tickets"]),
                    Price = ConvertUtility.ToInt64(row["price"]),
                    Status = ConvertUtility.ToString(row["status"])
                };
            }

            return result;
        }

        public CancelTicketResponse CancelTicket(long hostId, CancelTicketRequest value, out string status)
        {
            AdmissionServices admissionServices = new AdmissionServices();
            var ticket = admissionServices.GetTicketByTicketNo(hostId, value.TicketNumber);
            if (ticket == null)
            {
                status = CancelTicketStatuses.TicketNotFound;
                return null;
            }
            else
            {
                return CancelAndRefundTicket(hostId, ticket.TicketId.Value, out status);
            }
        }

        //TODO: performance optimize this method to cancel tickets in bulk
        public CancelTicketResponse CancelAndRefundTicket(long hostId, long ticketId, out string status)
        {
            CancelTicketResponse result = null;

            AdmissionServices admissionServices = new AdmissionServices();

            //Get ticket info
            var ticket = admissionServices.GetTicketById(ticketId);
            if(ticket == null)
            {
                status = CancelTicketStatuses.TicketNotFound;
                return null;
            }
            if(ticket.Status == TicketStatuses.Used)
            {
                status = CancelTicketStatuses.TicketAlreadyUsed;
                return null;
            }
            if (ticket.Status == TicketStatuses.Cancelled)
            {
                status = CancelTicketStatuses.TicketAlreadyCancelled;
                return null;
            }

            //Get event info
            var eventInfo = GetEventByRef(hostId, ticket.EventReferenceId);
            if(eventInfo == null)
            {
                status = CancelTicketStatuses.EventNotFound;
                return null;
            }
            //Validate event status
            if (eventInfo.Status != EventStatuses.Active)
            {
                status = CancelTicketStatuses.EventNotActive;
                return null;
            }
            //Validate if event start date is over
            if (DateTime.UtcNow >= eventInfo.StartDate)
            {
                status = CancelTicketStatuses.EventHasAlreadyStarted;
                return null;
            }

            //Get event tier
            EventsServices eventsServices = new EventsServices();
            var eventTier = eventsServices.GetEventTiersByRef(hostId, ticket.TierReferenceId);
            if (eventTier == null)
            {
                status = CancelTicketStatuses.TierNotFound;
                return null;
            }

            //Get customer info
            CustomerServices customerServices = new CustomerServices();
            var customer = customerServices.GetCustomerByRef(hostId, ticket.CustomerReferenceId);
            if(customer == null)
            {
                status = CancelTicketStatuses.CustomerNotFound;
                return null;
            }

            //Refund the customer using funds from the event address
            WalletServices walletServices = new WalletServices();
            walletServices.Transfer(eventInfo.WalletAddress, customer.WalletAddress, ticket.PaidPrice.Value, ticket.BookingId, "Refund");

            //Update ticket status to cancelled
            admissionServices.UpdateTicketStatus(ticketId, TicketStatuses.Cancelled);

            //Update ticket availability
            BookingServices bookingServices = new BookingServices();
            bookingServices.UpdateAvailableTickets(eventTier.TierId.Value, eventTier.AvailableTickets.Value + 1);
            
            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.CancelTicket(ticket.TicketAddress);

            status = CancelEventStatuses.Success;

            result = new CancelTicketResponse();
            result.EventTitle = ticket.EventTitle;
            result.TierTitle = ticket.TierTitle;
            result.PaidPrice = ticket.PaidPrice;
            result.NewTicketStatus = TicketStatuses.Cancelled;
            
            return result;
        }

    }
}