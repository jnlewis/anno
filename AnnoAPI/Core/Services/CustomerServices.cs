using AnnoAPI.Core.Contract;
using AnnoAPI.Core.Enum;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace AnnoAPI.Core.Services
{
    public class CustomerServices
    {
        MySqlUtility databaseAnno = null;

        public CustomerServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }

        /// <summary>
        /// Inserts a new customer record in database.
        /// </summary>
        public CreateCustomerResponse CreateCustomer(long hostId, CreateCustomerRequest value)
        {
            CreateCustomerResponse result = new CreateCustomerResponse();

            long newCustomerId = 0;

            //Generate address
            string customerAddress = HashUtility.GenerateHash();

            //Insert customer to database
            string sql = @"INSERT INTO customer (host_id, ref_id, address, record_status, created_date) 
                            VALUES (@host_id, @ref_id, @address, @record_status, @created_date)";

            sql = sql.Replace("@host_id", DataUtility.ToMySqlParam(hostId))
                    .Replace("@ref_id", DataUtility.ToMySqlParam(value.ReferenceId))
                    .Replace("@address", DataUtility.ToMySqlParam(customerAddress))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            this.databaseAnno.Execute(sql, out newCustomerId);

            //Create customer wallet
            WalletServices walletServices = new WalletServices();
            walletServices.CreateWallet(newCustomerId, WalletOwnerTypes.Customer, customerAddress);

            //Commit to blockchain
            IdentityServices identityService = new IdentityServices();
            BlockchainContract blockchainContract = new BlockchainContract();
            blockchainContract.CreateCustomer(identityService.AddressOf(IdentityServices.AddressTypes.Host, hostId), customerAddress, value.ReferenceId);

            //TODO: For demo purpose, give 1000 ANN tokens to new customers
            walletServices.Transfer(Config.OwnerScriptHash, customerAddress, 1000, null, "Demo");

            //Commit to blockchain
            blockchainContract.Transfer(Config.OwnerScriptHash, customerAddress, 1000);

            result.WalletAddress = customerAddress;
            result.WalletBalance = 1000;

            return result;
        }

        /// <summary>
        /// Gets all customer records by host.
        /// </summary>
        public List<Customer> GetCustomers(long hostId)
        {
            List<Customer> result = null;
            
            string sql = @"SELECT u.customer_id, u.ref_id, u.address as customer_address, w.balance, w.address
                                FROM customer u
                                INNER JOIN wallet w ON (w.owner_id = u.customer_id AND w.owner_type=@ownerType)
                                WHERE u.record_status=@recordStatus
                                AND w.record_status=@recordStatus
                                AND u.host_id=@hostId";

            sql = sql.Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                    .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Customer))
                    .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

            DataTable dt = databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                result = new List<Customer>();

                foreach (DataRow row in dt.Rows)
                {
                    result.Add(new Customer()
                    {
                        CustomerId = ConvertUtility.ToInt32(row["customer_id"]),
                        RefId = ConvertUtility.ToString(row["ref_id"]),
                        CustomerAddress = ConvertUtility.ToString(row["customer_address"]),
                        WalletBalance = ConvertUtility.ToInt64(row["balance"]),
                        WalletAddress = ConvertUtility.ToString(row["address"])
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the customer record by the reference Id.
        /// Returns null if record is not found.
        /// </summary>
        /// <param name="hostId">Caller Host Id.</param>
        /// <param name="refId">Reference Id.</param>
        /// <returns>Customer record, null if record not found.</returns>
        public Customer GetCustomerByRef(long hostId, string refId)
        {
            Customer result = null;

            string sql = @"SELECT u.customer_id, u.ref_id, u.address as customer_address, w.balance, w.address
                                FROM customer u
                                INNER JOIN wallet w ON (w.owner_id = u.customer_id AND w.owner_type=@ownerType)
                                WHERE u.record_status=@recordStatus
                                AND w.record_status=@recordStatus
                                AND u.host_id=@hostId 
                                AND u.ref_id=@refId";

            sql = sql
                .Replace("@hostId", DataUtility.ToMySqlParam(hostId))
                .Replace("@refId", DataUtility.ToMySqlParam(refId))
                .Replace("@ownerType", DataUtility.ToMySqlParam(WalletOwnerTypes.Customer))
                .Replace("@recordStatus", DataUtility.ToMySqlParam(RecordStatuses.Live));

            DataTable dt = this.databaseAnno.ExecuteAsDataTable(sql);
            if (DataUtility.HasRecord(dt))
            {
                DataRow row = dt.Rows[0];

                result = new Customer()
                {
                    CustomerId = ConvertUtility.ToInt32(row["customer_id"]),
                    RefId = ConvertUtility.ToString(row["ref_id"]),
                    CustomerAddress = ConvertUtility.ToString(row["customer_address"]),
                    WalletBalance = ConvertUtility.ToInt64(row["balance"]),
                    WalletAddress = ConvertUtility.ToString(row["address"])
                };
            }

            return result;
        }

    }
}