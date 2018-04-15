using Anno.Models.Entities;
using AnnoAPI.Core.Const;
using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Data;
using System.Linq;

namespace AnnoAPI.Core.Services
{
    public class HostServices
    {
        public HostServices()
        {
        }

        public static long? GetCallerHostId()
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from a in context.ApiKey
                           where a.api_key == RequestHeaders.API_KEY
                           select new { a.host_id })
                           .FirstOrDefault();

                if(data != null)
                {
                    return data.host_id;
                }
                else
                {
                    return null;
                }
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
            using (var context = new AnnoDBContext())
            {
                var data = (from a in context.Host
                            join b in context.ApiKey on a.host_id equals b.host_id
                            where b.api_key == apiKey
                            select new Host() {
                                host_id = a.host_id,
                                name = a.name,
                                address = a.address,
                                record_status = a.record_status,
                                created_date = a.created_date
                            })
                            .FirstOrDefault();

                return data;
            }
        }
        
        /// <summary>
        /// Inserts a new host record in database.
        /// </summary>
        public void CreateHost(CreateHostRequest value, out string newAPIKey)
        {
            long newHostId = 0;

            //Generate address
            string hostAddress = HashUtility.GenerateHash();

            using (var context = new AnnoDBContext())
            {
                //Insert host to database
                var newHost = new Host()
                {
                    name = value.Name,
                    address = hostAddress,
                    record_status = RecordStatuses.Live,
                    created_date = DateTime.UtcNow
                };
                context.Host.Add(newHost);
                context.SaveChanges();

                //Get the ID of the newly created host
                newHostId = newHost.host_id;
                
                //Generate new API key
                newAPIKey = Guid.NewGuid().ToString().Replace("-", "");

                //Insert into api_keys table
                context.ApiKey.Add(new ApiKey()
                {
                    host_id = newHostId,
                    api_key = newAPIKey,
                    record_status = RecordStatuses.Live,
                    created_date = DateTime.UtcNow
                });
                context.SaveChanges();

                //Create host wallet
                WalletServices walletService = new WalletServices();
                walletService.CreateWallet(newHostId, WalletOwnerTypes.Host, hostAddress);
            }

            //Commit to blockchain
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.CreateHost(hostAddress, value.Name);
        }

        public HostInfo GetHostInfoByAPIKey(string apiKey)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from h in context.Host
                            join k in context.ApiKey on h.host_id equals k.host_id
                            join w in context.Wallet on h.host_id equals w.owner_id //TODO: join owner type here
                            where w.owner_type == WalletOwnerTypes.Host
                            && k.record_status == RecordStatuses.Live
                            && h.record_status == RecordStatuses.Live
                            && w.record_status == RecordStatuses.Live
                            && k.api_key == apiKey
                            select new HostInfo()
                            {
                                HostId = h.host_id,
                                Name = h.name,
                                WalletAddress = w.address,
                                WalletBalance = w.balance
                            })
                            .FirstOrDefault();

                return data;
            }
        }

        public HostInfo GetHostInfoById(long hostId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from h in context.Host
                            join w in context.Wallet on h.host_id equals w.owner_id //TODO: join owner type here
                            where w.owner_type == WalletOwnerTypes.Host
                            && h.record_status == RecordStatuses.Live
                            && w.record_status == RecordStatuses.Live
                            && h.host_id == hostId
                            select new HostInfo()
                            {
                                HostId = h.host_id,
                                Name = h.name,
                                WalletAddress = w.address,
                                WalletBalance = w.balance
                            })
                            .FirstOrDefault();

                return data;
            }
        }

    }
}