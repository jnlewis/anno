using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Enum;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Data;

namespace AnnoAPI.Core.Services
{
    public class HostServices
    {
        MySqlUtility databaseAnno = null;

        public HostServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }

        public static long? GetCallerHostId()
        {
            HostServices hostService = new HostServices();
            var host = hostService.GetHostByAPIKey(RequestHeaders.API_KEY);

            if (host != null && host.HostId.HasValue)
            {
                return host.HostId.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the host record by the given api key.
        /// Returns null if record is not found.
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <returns>Host record, null if record is not found.</returns>
        public Host GetHostByAPIKey(string apiKey)
        {
            Host result = null;

            string sql = @"SELECT h.host_id, h.name
                            FROM host h
                            INNER JOIN api_keys k ON (k.host_id = h.host_id)
                            WHERE k.record_status=@recordStatus 
                            AND h.record_status=@recordStatus
                            AND api_key=@apiKey";

            sql = sql
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live))
                .Replace("@apiKey", DataUtility.ToMySqlParam(apiKey));

            DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new Host();
                result.HostId = ConvertUtility.ToInt32(row["host_id"]);
                result.Name = ConvertUtility.ToString(row["name"]);
            }

            return result;
        }
        
        /// <summary>
        /// Inserts a new host record in database.
        /// </summary>
        public void CreateHost(CreateHostRequest value, out string newAPIKey)
        {
            string sql = null;

            long newHostId = 0;

            //Generate address
            string hostAddress = HashUtility.GenerateHash();

            //Insert host to database
            sql = @"INSERT INTO host (name, address, record_status, created_date)
                    VALUES (@name, @address, @record_status, @created_date)";

            sql = sql.Replace("@name", DataUtility.ToMySqlParam(value.Name))
                    .Replace("@address", DataUtility.ToMySqlParam(hostAddress))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));
            
            this.databaseAnno.Execute(sql, out newHostId);

            //Generate new API key
            newAPIKey = Guid.NewGuid().ToString().Replace("-", "");

            //Insert into api_keys table
            sql = @"INSERT INTO api_keys(host_id, api_key, record_status, created_date)
                    VALUES (@host_id, @api_key, @record_status, @created_date)";

            sql = sql.Replace("@host_id", DataUtility.ToMySqlParam(newHostId))
                    .Replace("@api_key", DataUtility.ToMySqlParam(newAPIKey))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            this.databaseAnno.Execute(sql);

            //Create host wallet
            WalletServices walletService = new WalletServices();
            walletService.CreateWallet(newHostId, WalletOwnerTypes.Host, hostAddress);

            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.CreateHost(hostAddress, value.Name);
        }

        public HostInfo GetHostInfoByAPIKey(string apiKey)
        {
            HostInfo result = null;

            string sql = @"SELECT h.host_id, h.name, w.address, w.balance
                            FROM host h
                            INNER JOIN api_keys k ON (k.host_id = h.host_id)
                            INNER JOIN wallet w ON (w.owner_id = h.host_id AND w.owner_type=@ownerType)
                            WHERE k.record_status=@recordStatus 
                            AND h.record_status=@recordStatus 
                            AND w.record_status=@recordStatus
                            AND api_key=@apiKey";

            sql = sql
                .Replace("@apiKey", DataUtility.ToMySqlParam(apiKey))
                .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Host))
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));
            
            DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new HostInfo()
                {
                    HostId = ConvertUtility.ToInt32(row["host_id"]),
                    Name = ConvertUtility.ToString(row["name"]),
                    WalletAddress = ConvertUtility.ToString(row["address"]),
                    WalletBalance = ConvertUtility.ToInt64(row["balance"])
                };
            }

            return result;
        }

        public HostInfo GetHostInfoById(long hostId)
        {
            HostInfo result = null;

            string sql = @"SELECT h.host_id, h.name, w.address, w.balance
                            FROM host h
                            INNER JOIN wallet w ON (w.owner_id = h.host_id AND w.owner_type=@ownerType)
                            WHERE h.record_status=@recordStatus 
                            AND w.record_status=@recordStatus
                            AND h.host_id=@hostId";

            sql = sql
                .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Host))
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

            DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new HostInfo()
                {
                    HostId = ConvertUtility.ToInt32(row["host_id"]),
                    Name = ConvertUtility.ToString(row["name"]),
                    WalletAddress = ConvertUtility.ToString(row["address"]),
                    WalletBalance = ConvertUtility.ToInt64(row["balance"])
                };
            }

            return result;
        }

    }
}