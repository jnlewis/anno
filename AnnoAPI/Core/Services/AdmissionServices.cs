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
    public class AdmissionServices
    {
        MySqlUtility databaseAnno = null;

        public AdmissionServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
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

        public Ticket GetTicketByTicketNo(long hostId, string ticketNo)
        {
            Ticket result = null;

            string sql = @"SELECT b.booking_id, tic.ticket_id, tic.ticket_number, tic.seat_number, tic.status, tic.paid_price, tic.address as ticket_address,
                                b.confirmation_number, e.title as event_title, e.ref_id as event_ref, u.ref_id as customer_ref, t.title as tier_title, t.ref_id as tier_ref 
                                FROM customer_ticket tic
                                INNER JOIN customer_booking b ON (b.booking_id = tic.booking_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                INNER JOIN events_tier t ON (t.event_id = e.event_id AND tic.tier_id = t.tier_id)
                                INNER JOIN customer u ON (u.customer_id = b.customer_id)
                                WHERE b.record_status='Live'
                                AND e.record_status='Live'
                                AND tic.record_status='Live'
                                AND e.host_id=@hostId  
                                AND tic.ticket_number=@ticketNo";

            sql = sql
                .Replace("@hostId ", DataUtility.ToMySqlParam(hostId))
                .Replace("@ticketNo", DataUtility.ToMySqlParam(ticketNo));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new Ticket()
                {
                    BookingId = ConvertUtility.ToInt64(row["booking_id"]),
                    TicketId = ConvertUtility.ToInt64(row["ticket_id"]),
                    CustomerReferenceId = ConvertUtility.ToString(row["customer_ref"]),
                    EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                    TierReferenceId = ConvertUtility.ToString(row["tier_ref"]),
                    EventTitle = ConvertUtility.ToString(row["event_title"]),
                    TierTitle = ConvertUtility.ToString(row["tier_title"]),
                    TicketNo = ConvertUtility.ToString(row["ticket_number"]),
                    BookingConfirmationNo = ConvertUtility.ToString(row["confirmation_number"]),
                    Status = ConvertUtility.ToString(row["status"]),
                    PaidPrice = ConvertUtility.ToInt64(row["paid_price"]),
                    TicketAddress = ConvertUtility.ToString(row["ticket_address"])
                };
            }

            return result;
        }

        public Ticket GetTicketById(long ticketId)
        {
            Ticket result = null;

            string sql = @"SELECT tic.ticket_id, tic.ticket_number, tic.seat_number, tic.status, tic.paid_price, tic.address as ticket_address,
                                b.booking_id, b.confirmation_number, e.title as event_title, e.ref_id as event_ref, u.ref_id as customer_ref, t.title as tier_title, t.ref_id as tier_ref 
                                FROM customer_ticket tic
                                INNER JOIN customer_booking b ON (b.booking_id = tic.booking_id)
                                INNER JOIN events e ON (e.event_id = b.event_id)
                                INNER JOIN events_tier t ON (t.event_id = e.event_id AND tic.tier_id = t.tier_id)
                                INNER JOIN customer u ON (u.customer_id = b.customer_id)
                                WHERE b.record_status=@recordStatus
                                AND e.record_status=@recordStatus
                                AND tic.record_status=@recordStatus
                                AND tic.ticket_id=@ticketId";

            sql = sql
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                .Replace("@ticketId", DataUtility.ToMySqlParam(ticketId));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new Ticket()
                {
                    TicketId = ConvertUtility.ToInt64(row["ticket_id"]),
                    BookingId = ConvertUtility.ToInt64(row["booking_id"]),
                    CustomerReferenceId = ConvertUtility.ToString(row["customer_ref"]),
                    EventReferenceId = ConvertUtility.ToString(row["event_ref"]),
                    TierReferenceId = ConvertUtility.ToString(row["tier_ref"]),
                    EventTitle = ConvertUtility.ToString(row["event_title"]),
                    TierTitle = ConvertUtility.ToString(row["tier_title"]),
                    TicketNo = ConvertUtility.ToString(row["ticket_number"]),
                    BookingConfirmationNo = ConvertUtility.ToString(row["confirmation_number"]),
                    Status = ConvertUtility.ToString(row["status"]),
                    PaidPrice = ConvertUtility.ToInt64(row["paid_price"]),
                    TicketAddress = ConvertUtility.ToString(row["ticket_address"])
                };
            }

            return result;
        }
        
        public void UpdateTicketStatus(long ticketId, string status)
        {
            string sql = @"UPDATE customer_ticket SET status=@status WHERE ticket_id=@ticketId";

            sql = sql.Replace("@status", DataUtility.ToMySqlParam(status))
                    .Replace("@ticketId", DataUtility.ToMySqlParam(ticketId));

            this.databaseAnno.Execute(sql);
        }
        
    }
}