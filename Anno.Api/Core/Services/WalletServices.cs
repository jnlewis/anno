using Anno.Models.Entities;
using AnnoAPI.Core.Const;
using System;
using System.Linq;

namespace AnnoAPI.Core.Services
{
    public class WalletServices
    {
        public WalletServices()
        {
        }

        public void CreateWallet(long ownerId, string ownerType, string address)
        {
            using (var context = new AnnoDBContext())
            {
                context.Wallet.Add(new Wallet()
                {
                    owner_id = ownerId,
                    owner_type = ownerType,
                    address = address,
                    balance = 0,
                    record_status = RecordStatuses.Live,
                    created_date = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        }
        
        public void Transfer(string fromAddress, string toAddress, decimal amount, long? bookingId, string description)
        {
            using (var context = new AnnoDBContext())
            {
                //Insert transaction to database
                context.Transaction.Add(new Transaction()
                {
                    transaction_datetime = DateTime.UtcNow,
                    address_from = fromAddress,
                    address_to = toAddress,
                    amount = amount,
                    booking_id = bookingId,
                    description = description,
                    created_date = DateTime.UtcNow
                });

                //Update sender wallet
                var fromWallet = context.Wallet.Where(x => x.address == fromAddress).FirstOrDefault();
                if(fromWallet != null)
                {
                    fromWallet.balance = fromWallet.balance - amount;
                }

                //Update recipient wallet
                var toWallet = context.Wallet.Where(x => x.address == toAddress).FirstOrDefault();
                if (toWallet != null)
                {
                    toWallet.balance = toWallet.balance + amount;
                }

                context.SaveChanges();
            }
        }
    }
}