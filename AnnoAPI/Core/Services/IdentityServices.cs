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
    public class IdentityServices
    {
        MySqlUtility databaseAnno = null;

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

        public IdentityServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }
        
        public bool IsRefIdExists(string refIdType, long hostId, string refId)
        {
            if (refIdType == RefIdTypes.Customer)
            {
                string sql = @"SELECT ref_id FROM customer WHERE record_status=@recordStatus AND host_id=@hostId AND ref_id=@refId";
                sql = sql
                    .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                    .Replace("@refId", DataUtility.ToMySqlParam(refId))
                    .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                return DataUtility.HasRecord(dt);
            }
            else if (refIdType == RefIdTypes.Event)
            {
                string sql = @"SELECT ref_id FROM events WHERE record_status=@recordStatus AND host_id=@hostId AND ref_id=@refId";
                sql = sql
                    .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                    .Replace("@refId", DataUtility.ToMySqlParam(refId))
                    .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                return DataUtility.HasRecord(dt);
            }
            else if (refIdType == RefIdTypes.EventTier)
            {
                string sql = @"SELECT ref_id FROM events_tier WHERE record_status=@recordStatus AND host_id=@hostId AND ref_id=@refId";
                sql = sql
                    .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                    .Replace("@refId", DataUtility.ToMySqlParam(refId))
                    .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                return DataUtility.HasRecord(dt);
            }
            else
            {
                throw new Exception("Unrecognized reference ID type: " + refIdType);
            }
        }
        
        public string AddressOf(string addressType, long id)
        {
            string result = null;

            if (addressType == AddressTypes.Host)
            {
                string sql = @"SELECT address FROM host WHERE host_id=@id";
                sql = sql.Replace("@id", DataUtility.ToMySqlParam(id));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                if (DataUtility.HasRecord(dt))
                    result = dt.Rows[0]["address"].ToString();
            }
            else if (addressType == AddressTypes.Customer)
            {
                string sql = @"SELECT address FROM customer WHERE customer_id=@id";
                sql = sql.Replace("@id", DataUtility.ToMySqlParam(id));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                if (DataUtility.HasRecord(dt))
                    result = dt.Rows[0]["address"].ToString();
            }
            else if (addressType == AddressTypes.Event)
            {
                string sql = @"SELECT address FROM events WHERE event_id=@id";
                sql = sql.Replace("@id", DataUtility.ToMySqlParam(id));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                if (DataUtility.HasRecord(dt))
                    result = dt.Rows[0]["address"].ToString();
            }
            else if (addressType == AddressTypes.EventTier)
            {
                string sql = @"SELECT address FROM events_tier WHERE tier_id=@id";
                sql = sql.Replace("@id", DataUtility.ToMySqlParam(id));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                if (DataUtility.HasRecord(dt))
                    result = dt.Rows[0]["address"].ToString();
            }
            else if (addressType == AddressTypes.Ticket)
            {
                string sql = @"SELECT address FROM customer_ticket WHERE ticket_id=@id";
                sql = sql.Replace("@id", DataUtility.ToMySqlParam(id));

                DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
                if (DataUtility.HasRecord(dt))
                    result = dt.Rows[0]["address"].ToString();
            }
            else
            {
                throw new Exception("Unrecognized address type: " + addressType);
            }

            return result;

        }
    }
}