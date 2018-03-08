using Anno.Models.Entities;
using AnnoAPI.Core.Const;
using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AnnoAPI.Core.Services
{
    public class EventsServices
    {
        public EventsServices()
        {
        }

        /// <summary>
        /// Inserts a new event record in the Events table.
        /// </summary>
        public void CreateEvent(long hostId, CreateEventsRequest value)
        {
            long newEventId = 0;
            List<string> tierAddresses = new List<string>();

            //Generate address
            string eventAddress = HashUtility.GenerateHash();

            using (var context = new AnnoDBContext())
            {
                //Insert event to database
                var newEvent = new Events()
                {
                    host_id = hostId,
                    ref_id = value.ReferenceId,
                    title = value.Title,
                    description = value.Description,
                    start_date = value.StartDate,
                    status = "Active",
                    address = eventAddress,
                    record_status = RecordStatuses.Pending,
                    created_date = DateTime.UtcNow
                };
                context.Events.Add(newEvent);
                context.SaveChanges();

                //Get the ID of the newly created record
                newEventId = newEvent.event_id;

                //Insert event tiers to database
                foreach (var tier in value.Tiers)
                {
                    //Generate address
                    string tierAddress = HashUtility.GenerateHash();

                    //Insert event to database
                    var newEventTier = new EventsTier()
                    {
                        host_id = hostId,
                        event_id = newEventId,
                        ref_id = tier.ReferenceId,
                        title = tier.Title,
                        description = tier.Description,
                        total_tickets = tier.TotalTickets,
                        available_tickets = tier.TotalTickets, //available tickets is set to total tickets for initial insert
                        price = Convert.ToDecimal(tier.Price),
                        status = "Active",
                        address = tierAddress,
                        record_status = RecordStatuses.Live,
                        created_date = DateTime.UtcNow
                    };
                    context.EventsTier.Add(newEventTier);
                    context.SaveChanges();

                    tierAddresses.Add(tierAddress);
                }

                //Update event record status to live
                var record = context.Events.SingleOrDefault(x => x.event_id == newEventId);
                if (record != null)
                {
                    record.record_status = RecordStatuses.Live;
                    context.SaveChanges();
                }
            }
            
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
            using (var context = new AnnoDBContext())
            {
                var record = context.Events.SingleOrDefault(x => 
                                x.host_id == hostId && 
                                x.ref_id == value.ReferenceId && 
                                x.record_status == RecordStatuses.Live);
                if (record != null)
                {
                    record.status = value.NewStatus;
                    context.SaveChanges();
                }
            }
        }

        public string CancelEvent(long hostId, CancelEventRequest value)
        {
            //Validate if event
            EventsServices eventsServices = new EventsServices();
            EventInfo eventInfo = eventsServices.GetEventByRef(hostId, value.ReferenceId);
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

        public List<EventInfo> GetEvents(long hostId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from e in context.Events
                            join w in context.Wallet on e.event_id equals w.owner_id
                            where w.owner_type == WalletOwnerTypes.Event
                            && e.record_status == RecordStatuses.Live
                            && w.record_status == RecordStatuses.Live
                            && e.host_id == hostId
                            select new EventInfo()
                            {
                                EventId = e.event_id,
                                ReferenceId = e.ref_id,
                                Title = e.title,
                                Description = e.description,
                                Status = e.status,
                                StartDate = e.start_date,
                                EventAddress = e.address,
                                WalletAddress = w.address,
                                WalletBalance = w.balance
                            }).ToList();

                return data;
            }
        }

        public EventInfo GetEventByRef(long hostId, string refId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from e in context.Events
                            join w in context.Wallet on e.event_id equals w.owner_id
                            where w.owner_type == WalletOwnerTypes.Event
                            && e.record_status == RecordStatuses.Live
                            && w.record_status == RecordStatuses.Live
                            && e.host_id == hostId
                            && e.ref_id == refId
                            select new EventInfo()
                            {
                                EventId = e.event_id,
                                ReferenceId = e.ref_id,
                                Title = e.title,
                                Description = e.description,
                                Status = e.status,
                                StartDate = e.start_date,
                                EventAddress = e.address,
                                WalletAddress = w.address,
                                WalletBalance = w.balance
                            })
                            .FirstOrDefault();

                return data;
            }
        }

        public List<EventTierInfo> GetEventTiersByEventId(long eventId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from t in context.EventsTier
                            where t.record_status == RecordStatuses.Live
                            && t.event_id == eventId
                            select new EventTierInfo()
                            {
                                TierId = t.tier_id,
                                EventId = t.event_id,
                                ReferenceId = t.ref_id,
                                Title = t.title,
                                Description = t.description,
                                TotalTickets = t.total_tickets,
                                AvailableTickets = t.available_tickets,
                                Price = t.price,
                                Status = t.status
                            })
                            .ToList();

                return data;
            }
        }

        public EventTierInfo GetEventTiersByRef(long hostId, string refId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from t in context.EventsTier
                            where t.record_status == RecordStatuses.Live
                            && t.host_id == hostId
                            && t.ref_id == refId
                            select new EventTierInfo()
                            {
                                TierId = t.tier_id,
                                EventId = t.event_id,
                                ReferenceId = t.ref_id,
                                Title = t.title,
                                Description = t.description,
                                TotalTickets = t.total_tickets,
                                AvailableTickets = t.available_tickets,
                                Price = t.price,
                                Status = t.status
                            })
                            .FirstOrDefault();

                return data;
            }
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