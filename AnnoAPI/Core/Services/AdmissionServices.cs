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
    public class AdmissionServices
    {
        public AdmissionServices()
        {
        }

        public RedeemTicketReponse UseTicket(long hostId, string ticketNo, out string status)
        {
            RedeemTicketReponse result = null;

            //Validate if ticket exists
            var ticket = GetTicketByTicketNo(hostId, ticketNo);
            if (ticket == null)
            {
                status = RedeemTicketStatuses.TicketNotFound;
                return null;
            }

            //Check ticket status
            if (ticket.Status == TicketStatuses.Used)
            {
                status = RedeemTicketStatuses.TicketAlreadyUsed;
                return null;
            }
            else if (ticket.Status != TicketStatuses.Active)
            {
                status = RedeemTicketStatuses.TicketIsInactive;
                return null;
            }

            //Update ticket status
            UpdateTicketStatus(ticket.TicketId.Value, TicketStatuses.Used);

            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.RedeemTicket(ticket.TicketAddress);

            status = BookEventStatuses.Success;

            result = new RedeemTicketReponse();
            result.EventTitle = ticket.EventTitle;
            result.TierTitle = ticket.TierTitle;
            result.PaidPrice = ticket.PaidPrice;
            result.NewTicketStatus = TicketStatuses.Used;

            return result;
        }

        public TicketInfo GetTicketByTicketNo(long hostId, string ticketNo)
        {
            using(var context = new AnnoDBContext())
            {
                var data = (from tic in context.CustomerTicket
                            join b in context.CustomerBooking on tic.booking_id equals b.booking_id
                            join e in context.Events on b.event_id equals e.event_id
                            join t in context.EventsTier on new { Key1 = e.event_id, Key2 = tic.tier_id } equals new { Key1 = t.event_id, Key2 = t.tier_id }
                            join u in context.Customer on b.customer_id equals u.customer_id
                            where b.record_status == RecordStatuses.Live
                            && e.record_status == RecordStatuses.Live
                            && tic.record_status == RecordStatuses.Live
                            && e.host_id == hostId
                            && tic.ticket_number == ticketNo
                            select new TicketInfo
                            {
                                BookingId = b.booking_id,
                                TicketId = tic.ticket_id,
                                CustomerReferenceId = u.ref_id,
                                EventReferenceId = e.ref_id,
                                TierReferenceId = t.ref_id, 
                                EventTitle = e.title,
                                TierTitle = t.title, 
                                TicketNo = tic.ticket_number, 
                                BookingConfirmationNo = b.confirmation_number, 
                                Status = tic.status,
                                PaidPrice = tic.paid_price,
                                TicketAddress = tic.address
                            }).FirstOrDefault();

                return data;
            }
        }

        public TicketInfo GetTicketById(long ticketId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from tic in context.CustomerTicket
                            join b in context.CustomerBooking on tic.booking_id equals b.booking_id
                            join e in context.Events on b.event_id equals e.event_id
                            join t in context.EventsTier on new { Key1 = e.event_id, Key2 = tic.tier_id } equals new { Key1 = t.event_id, Key2 = t.tier_id }
                            join u in context.Customer on b.customer_id equals u.customer_id
                            where b.record_status == RecordStatuses.Live
                            && e.record_status == RecordStatuses.Live
                            && tic.record_status == RecordStatuses.Live
                            && tic.ticket_id == ticketId
                            select new TicketInfo
                            {
                                BookingId = b.booking_id,
                                TicketId = tic.ticket_id,
                                CustomerReferenceId = u.ref_id,
                                EventReferenceId = e.ref_id,
                                TierReferenceId = t.ref_id,
                                EventTitle = e.title,
                                TierTitle = t.title,
                                TicketNo = tic.ticket_number,
                                BookingConfirmationNo = b.confirmation_number,
                                Status = tic.status,
                                PaidPrice = tic.paid_price,
                                TicketAddress = tic.address
                            }).FirstOrDefault();

                return data;
            }
        }

        public void UpdateTicketStatus(long ticketId, string status)
        {
            using (var context = new AnnoDBContext())
            {
                var record = context.CustomerTicket.SingleOrDefault(x => x.ticket_id == ticketId);
                if (record != null)
                {
                    record.status = status;
                    context.SaveChanges();
                }
            }
        }
    }
}