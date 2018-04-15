using System;
using System.Collections.Generic;
using Anno.Api.Core.Utility;
using NeoLux;

namespace Anno.Api.Core.Contract
{
    public class BlockchainContract
    {
        private NeoAPI.Net environment = NeoAPI.Net.Test;
        private string privateKey = null; //HEX
        private string smartContractScriptHash = null;

        public BlockchainContract()
        {
            privateKey = Config.ContractPrivateKeyHEX;
            smartContractScriptHash = Config.ContractScriptHash;
        }

        #region Helpers

        private bool CallContract(string method, object[] values)
        {
            //NOTE: NeoLux does not support CoZ testnet at the moment.
            //Modified source to add temporary support by hardcoding a single node.
            //Keep an eye out for updates.

            //Audit log
            Log.Info(string.Format("CallContract: CommitToBlockchain:{0}, method:{1}, params:{2}", Config.CommitToBlockchain.ToString(), method, String.Join(",", values)));

            if (Config.CommitToBlockchain)
            {
                try
                {
                    var key = new KeyPair(ConvertUtility.HexToBytes(privateKey));
                    var result = NeoAPI.CallContract(environment, key, smartContractScriptHash, method, values);

                    if (result == false)
                    {
                        Log.Error("CallContract: Received false on method " + method);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error("CallContract: Failed on method " + method + ". " + ex.Message);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Serialize a dictionary into string with the structure:
        /// key1:value1,key2:value2
        /// </summary>
        /// <param name="values">Dictionary values to serialize.</param>
        /// <param name="keys">Keys in the dictionary values to serialize. Null value will serialize all keys in the dictionary values.</param>
        /// <returns>Serialized string.</returns>
        private string Serialize(Dictionary<string, string> values, List<string> keys = null)
        {
            string result = "";

            if (keys == null)
            {
                //Serialize all items in the dictionary
                keys = new List<string>(values.Keys);   
            }

            for (int i = 0; i < keys.Count; i++)
            {
                //TODO: Temporary solution to filter out : and , to avoid deserialization issues within the smart contract.
                //      These values should be encoded/decoded instead.
                result += string.Format("{0}:{1}", 
                    keys[i].Replace(":", "").Replace(",", ""), 
                    values[keys[i]].Replace(":", "").Replace(",", ""));

                if (i != keys.Count - 1) //not last item
                    result += ",";
            }

            return result;
        }

        #endregion
        
        public void Transfer(string fromAddress, string toAddress, long amount)
        {
            List<object> param = new List<object>()
            {
                fromAddress,
                toAddress,
                amount
            };

            //bool Transfer(byte[] from, byte[] to, BigInteger amount)
            CallContract("transfer", param.ToArray());
        }
        
        public void CreateHost(string hostAddress, string name)
        {
            /*
                "name"
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("name", name);

            List<object> param = new List<object>()
            {
                hostAddress,
                Serialize(data)
            };

            //bool CreateHost(byte[] hostAddress, string data)
            CallContract("createHost", param.ToArray());
        }
        
        public void CreateCustomer(string hostAddress, string customerAddress, string refId)
        {
            /*
                "host_address",
                "ref_id"
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("host_address", hostAddress);
            data.Add("ref_id", refId);

            List<object> param = new List<object>()
            {
                customerAddress,
                Serialize(data)
            };

            //bool CreateCustomer(byte[] customerAddress, string data)
            CallContract("createCustomer", param.ToArray());
        }
        
        public void CreateEvent(string eventAddress, string hostAddress, string refId, string title, DateTime startDate, string status)
        {
            /*
                "host_address",
                "ref_id",
                "title",
                "start_date",
                "status"
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("host_address", hostAddress);
            data.Add("ref_id", refId);
            data.Add("title", title);
            data.Add("start_date", DateTimeUtility.ToUnixTime(startDate).ToString());
            data.Add("status", status);

            List<object> param = new List<object>()
            {
                eventAddress,
                Serialize(data)
            };

            //bool CreateEvent(byte[] eventAddress, string data)
            CallContract("createEvent", param.ToArray());
        }
        
        public void CreateEventTier(string tierAddress, string hostAddress, string eventAddress, string refId, string title, int totalTickets, int availableTickets, long price)
        {
            /*
                "host_address",
                "event_address",
                "ref_id",
                "title",
                "total_tickets",
                "available_tickets",
                "price"
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("host_address", hostAddress);
            data.Add("event_address", eventAddress);
            data.Add("ref_id", refId);
            data.Add("title", title);
            data.Add("total_tickets", totalTickets.ToString());
            data.Add("available_tickets", availableTickets.ToString());
            data.Add("price", price.ToString());

            List<object> param = new List<object>()
            {
                tierAddress,
                Serialize(data)
            };

            //bool CreateEventTier(byte[] tierAddress, string data)
            CallContract("createEventTier", param.ToArray());
        }
        
        public void BookEventTier(string customerAddress, string eventAddress, string tierAddress, int quantity, List<string> ticketNumbers, List<string> ticketAddresses)
        {
            /*
                "quantity",
                "ticket_numbers",
                "ticket_addresses"
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("quantity", quantity.ToString());
            data.Add("ticket_numbers", string.Join(";", ticketNumbers));
            data.Add("ticket_address", string.Join(";", ticketAddresses));

            List<object> param = new List<object>()
            {
                customerAddress,
                eventAddress,
                tierAddress,
                Serialize(data)
            };

            //bool BookEventTier(byte[] customerAddress, byte[] eventAddress, byte[] tierAddress, string bookingDetails)
            CallContract("bookEventTier", param.ToArray());
        }

        public void RedeemTicket(string ticketAddress)
        {
            List<object> param = new List<object>()
            {
                ticketAddress
            };

            //RedeemTicket(byte[] ticketAddress)
            CallContract("redeemTicket", param.ToArray());
        }

        public void CancelTicket(string ticketAddress)
        {
            List<object> param = new List<object>()
            {
                ticketAddress
            };

            //bool CancelTicket(byte[] ticketAddress)
            CallContract("cancelTicket", param.ToArray());
        }
        
        public void ClaimEarnings(string eventAddress)
        {
            List<object> param = new List<object>()
            {
                eventAddress
            };

            //bool ClaimEarnings(byte[] eventAddress)
            CallContract("claimEarnings", param.ToArray());
        }
        
    }
}