using Anno.Models.Entities;
using Anno.Api.Core.Const;
using Anno.Api.Core.Contract;
using Anno.Api.Core.Utility;
using Anno.Api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Anno.Api.Core.Services
{
    public class CustomerServices
    {
        public CustomerServices()
        {
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

            using (var context = new AnnoDBContext())
            {
                //Insert customer to database
                Customer customer = new Customer()
                {
                    host_id = hostId,
                    ref_id = value.ReferenceId,
                    address = customerAddress,
                    record_status = RecordStatuses.Live,
                    created_date = DateTime.UtcNow
                };
                context.Customer.Add(customer);
                context.SaveChanges();

                //Get the Id of the newly created customer
                newCustomerId = customer.customer_id;
            }
            
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
        public List<CustomerInfo> GetCustomers(long hostId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from u in context.Customer
                           join w in context.Wallet on u.customer_id equals w.owner_id
                           where w.owner_type == WalletOwnerTypes.Customer
                           && u.record_status == RecordStatuses.Live
                           && w.record_status == RecordStatuses.Live
                           && u.host_id == hostId
                           select new CustomerInfo()
                           {
                               CustomerId = u.customer_id,
                               RefId = u.ref_id,
                               CustomerAddress = u.address,
                               WalletBalance = w.balance,
                               WalletAddress = w.address
                           }).ToList();

                return data;
            }
        }

        /// <summary>
        /// Gets the customer record by the reference Id.
        /// Returns null if record is not found.
        /// </summary>
        /// <param name="hostId">Caller Host Id.</param>
        /// <param name="refId">Reference Id.</param>
        /// <returns>Customer record, null if record not found.</returns>
        public CustomerInfo GetCustomerByRef(long hostId, string refId)
        {
            using (var context = new AnnoDBContext())
            {
                var data = (from u in context.Customer
                            join w in context.Wallet on u.customer_id equals w.owner_id
                            where w.owner_type == WalletOwnerTypes.Customer
                            && u.record_status == RecordStatuses.Live
                            && w.record_status == RecordStatuses.Live
                            && u.host_id == hostId
                            && u.ref_id == refId
                            select new CustomerInfo()
                            {
                                CustomerId = u.customer_id,
                                RefId = u.ref_id,
                                CustomerAddress = u.address,
                                WalletBalance = w.balance,
                                WalletAddress = w.address
                            }).FirstOrDefault();

                return data;
            }
        }

    }
}