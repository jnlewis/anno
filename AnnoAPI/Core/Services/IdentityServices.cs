using Anno.Models.Entities;
using AnnoAPI.Core.Const;
using AnnoAPI.Core.Utility;
using System;
using System.Data;
using System.Linq;

namespace AnnoAPI.Core.Services
{
    public class IdentityServices
    {
        public IdentityServices()
        {
        }

        public static class RefIdTypes
        {
            public static string Customer = "Customer";
            public static string Event = "Event";
            public static string EventTier = "EventTier";
        }

        public static class AddressTypes
        {
            public static string Host = "Host";
            public static string Customer = "Customer";
            public static string Event = "Event";
            public static string EventTier = "EventTier";
            public static string Ticket = "Ticket";
        }

        public bool IsRefIdExists(string refIdType, long hostId, string refId)
        {
            using (var context = new AnnoDBContext())
            {
                if (refIdType == RefIdTypes.Customer)
                {
                    var data = (from a in context.Customer
                               where a.record_status == RecordStatuses.Live
                               && a.host_id == hostId
                               && a.ref_id == refId
                               select a.ref_id)
                               .FirstOrDefault();

                    return (data != null);
                }
                else if (refIdType == RefIdTypes.Event)
                {
                    var data = (from a in context.Events
                                where a.record_status == RecordStatuses.Live
                                && a.host_id == hostId
                                && a.ref_id == refId
                                select a.ref_id)
                               .FirstOrDefault();

                    return (data != null);
                }
                else if (refIdType == RefIdTypes.EventTier)
                {
                    var data = (from a in context.EventsTier
                                where a.record_status == RecordStatuses.Live
                                && a.host_id == hostId
                                && a.ref_id == refId
                                select a.ref_id)
                               .FirstOrDefault();

                    return (data != null);
                }
                else
                {
                    throw new ArgumentException("Unrecognized reference ID type", refIdType);
                }
            }
        }
        
        public string AddressOf(string addressType, long id)
        {
            string result = null;

            using (var context = new AnnoDBContext())
            {
                if (addressType == AddressTypes.Host)
                {
                    var data = context.Host.Where(x => x.host_id == id).FirstOrDefault();
                    if (data != null)
                        result = data.address;
                }
                else if (addressType == AddressTypes.Customer)
                {
                    var data = context.Customer.Where(x => x.customer_id == id).FirstOrDefault();
                    if (data != null)
                        result = data.address;
                }
                else if (addressType == AddressTypes.Event)
                {
                    var data = context.Events.Where(x => x.event_id == id).FirstOrDefault();
                    if (data != null)
                        result = data.address;
                }
                else if (addressType == AddressTypes.EventTier)
                {
                    var data = context.EventsTier.Where(x => x.tier_id == id).FirstOrDefault();
                    if (data != null)
                        result = data.address;
                }
                else if (addressType == AddressTypes.Ticket)
                {
                    var data = context.CustomerTicket.Where(x => x.ticket_id == id).FirstOrDefault();
                    if (data != null)
                        result = data.address;
                }
                else
                {
                    throw new ArgumentException("Unrecognized address type", addressType);
                }
            }

            return result;

        }
    }
}