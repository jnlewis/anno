using AnnoAPI.Core.Enum;
using AnnoAPI.Core.Utility;
using AnnoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace AnnoAPI.Core.Services
{
    public class WalletServices
    {
        MySqlUtility databaseAnno = null;

        public WalletServices()
        {
            this.databaseAnno = new MySqlUtility(Config.ConnectionString_Anno);
        }

        public void CreateWallet(long ownerId, string ownerType, string address)
        {
            string sql = null;
            
            //Insert wallet to database
            sql = @"INSERT INTO wallet (owner_id, owner_type, address, balance, record_status, created_date) 
                            VALUES (@ownerId, @ownerType, @address, @balance, @record_status, @created_date)";

            sql = sql.Replace("@ownerId", DataUtility.ToMySqlParam(ownerId))
                    .Replace("@ownerType", DataUtility.ToMySqlParam(ownerType))
                    .Replace("@address", DataUtility.ToMySqlParam(address))
                    .Replace("@balance", DataUtility.ToMySqlParam(0))
                    .Replace("@record_status", DataUtility.ToMySqlParam(RecordStatuses.Live))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));
            
            this.databaseAnno.Execute(sql);
        }

        public void Transfer(string fromAddress, string toAddress, long amount, long? bookingId, string description)
        {
            List<string> transactions = new List<string>();
            string sql = null;
            
            //Insert transaction to database
            sql = @"INSERT INTO transactions (transaction_datetime, address_from, address_to, amount, booking_id, description, created_date) 
                            VALUES (@transaction_datetime, @address_from, @address_to, @amount, @booking_id, @description, @created_date)";

            sql = sql.Replace("@transaction_datetime", DataUtility.ToMySqlParam(DateTime.UtcNow))
                    .Replace("@address_from", DataUtility.ToMySqlParam(fromAddress))
                    .Replace("@address_to", DataUtility.ToMySqlParam(toAddress))
                    .Replace("@amount", DataUtility.ToMySqlParam(amount))
                    .Replace("@booking_id", DataUtility.ToMySqlParam(bookingId))
                    .Replace("@description", DataUtility.ToMySqlParam(description))
                    .Replace("@created_date", DataUtility.ToMySqlParam(DateTime.UtcNow));

            transactions.Add(sql);
            
            //Update sender wallet
            sql = @"UPDATE wallet SET balance=balance-@amount WHERE address=@address";
            sql = sql.Replace("@amount", DataUtility.ToMySqlParam(amount))
                    .Replace("@address", DataUtility.ToMySqlParam(fromAddress));

            transactions.Add(sql);
            
            //Update recipient wallet
            sql = @"UPDATE wallet SET balance=balance+@amount WHERE address=@address";
            sql = sql.Replace("@amount", DataUtility.ToMySqlParam(amount))
                    .Replace("@address", DataUtility.ToMySqlParam(toAddress));

            transactions.Add(sql);

            this.databaseAnno.ExecuteTransaction(transactions.ToArray());
        }
    }
}