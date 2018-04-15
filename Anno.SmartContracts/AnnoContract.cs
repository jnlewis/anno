/// <summary>
/// MIT License
/// 
/// Copyright(c) 2018 Jeffrey Lewis
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </summary>

using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

/// <summary>
/// ANNO - Decentralized Ticket Booking and Admission Control Platform
/// 
/// Anno is a decentralized booking management platform with the aim to improve the integrity 
/// of online booking by providing an easy solution for businesses to utilize the 
/// programmable logic and immutability of smart contracts on the NEO blockchain.
/// 
/// Main Features:
/// Ticket sales and booking, payment transactions, ticket verification and redemption, cancellations and refunds.
/// 
/// For more details on the project visit:
/// Official Website: http://anno.network
/// Source Codes & Documents: https://github.com/jnlewis/anno
/// 
/// </summary>
namespace AnnoSmartContract
{
    public class AnnoContract : SmartContract
    {
        //Token Settings
        public static string Name() => "Anno";
        public static string Symbol() => "ANO";
        public static byte Decimals() => 8;
        private const ulong factor = 100000000; //decided by Decimals()

        //ICO Settings
        private static readonly byte[] neo_asset_id = { 155, 124, 255, 218, 166, 116, 190, 174, 15, 147, 14, 190, 96, 133, 175, 144, 147, 229, 254, 86, 179, 74, 92, 34, 12, 205, 207, 110, 252, 51, 111, 197 };
        private const ulong total_amount = 100000000 * factor; // total token amount
        private const ulong pre_ico_cap = 30000000 * factor; // pre ico token amount

        //[DisplayName("transfer")]
        //public static event Action<byte[], byte[], BigInteger> Transferred;

        //[DisplayName("refund")]
        //public static event Action<byte[], BigInteger> Refund;

        /// <summary>
        /// Main method of a contract.
        /// </summary>
        /// <param name="operation">Method to invoke.</param>
        /// <param name="args">Method parameters.</param>
        /// <returns>Method's return value or false if operation is invalid.</returns>
        public static object Main(string operation, params object[] args)
        {
            if (operation == "name")
                return Name();
            if (operation == "symbol")
                return Symbol();
            if (operation == "decimals")
                return Decimals();
            if (operation == "totalsupply")
                return TotalSupply();
            if (operation == "owner")
                return Owner();

            if (args.Length > 0)
            {
                if (operation == "deploy")
                    return Deploy((byte[])args[0]);

                if (operation == "balanceOf")
                    return BalanceOf((byte[])args[0]);

                if (operation == "transfer")
                    return Transfer((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);

                if (operation == "createHost")
                    return CreateHost((byte[])args[0], (string)args[1]);

                if (operation == "createCustomer")
                    return CreateCustomer((byte[])args[0], (string)args[1]);
                
                if (operation == "createEvent")
                    return CreateEvent((byte[])args[0], (string)args[1]);

                if (operation == "createEventTier")
                    return CreateEventTier((byte[])args[0], (string)args[1]);

                if (operation == "bookEventTier")
                    return BookEventTier((byte[])args[0], (byte[])args[1], (byte[])args[2], (string)args[3]);

                if (operation == "redeemTicket")
                    return RedeemTicket((byte[])args[0]);

                if (operation == "cancelTicket")
                    return CancelTicket((byte[])args[0]);
                
                if (operation == "claimEarnings")
                    return ClaimEarnings((byte[])args[0]);
            }

            return false;
        }

        #region Storage Key Prefixes
        
        // Storage Key Prefix are used for storing different categories of data on the blockchain
        // by prefixing a unique character to the storage key.
        private static string PREFIX_ACCOUNT = "A";
        private static string PREFIX_HOST = "H";
        private static string PREFIX_CUSTOMER = "C";
        private static string PREFIX_EVENT = "E";
        private static string PREFIX_EVENTTIER = "T";
        private static string PREFIX_TICKET = "I";

        #endregion

        #region dApp Methods
        
        public static bool Deploy(byte[] account)
        {
            byte[] supplyCheck = Storage.Get(Storage.CurrentContext, "totalsupply");

            if (supplyCheck == null)
            {
                Storage.Put(Storage.CurrentContext, "owner", account);
                byte[] owner = Storage.Get(Storage.CurrentContext, "owner");
                Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, owner), pre_ico_cap);
                Storage.Put(Storage.CurrentContext, "totalsupply", pre_ico_cap);
                //Transferred(null, owner, pre_ico_cap);
                return true;
            }
            return false;
        }

        private static BigInteger TotalSupply()
        {
            return Storage.Get(Storage.CurrentContext, "totalsupply").AsBigInteger();
        }

        private static string Owner()
        {
            return Storage.Get(Storage.CurrentContext, "owner").AsString();
        }

        private static BigInteger BalanceOf(byte[] account)
        {
            byte[] balance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, account));

            if (balance == null)
                return 0;

            return balance.AsBigInteger();
        }

        /// <summary>
        /// Transfers an amount from one account to another.
        /// </summary>
        private static bool Transfer(byte[] from, byte[] to, BigInteger amount)
        {
            if (amount >= 0)
            {
                if (from == to)
                    return true;

                BigInteger senderBalance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, from)).AsBigInteger();
                BigInteger recipientBalance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, to)).AsBigInteger();
                
                if (senderBalance >= amount)
                {
                    Runtime.Log("Transfer: Starting transfer...");

                    BigInteger newSenderBalance = senderBalance - amount;
                    BigInteger newRecipientBalance = recipientBalance + amount;

                    Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, from), newSenderBalance);
                    Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, to), newRecipientBalance);
                    
                    Runtime.Notify("AnnoBalance", from, newSenderBalance);
                    Runtime.Notify("AnnoBalance", to, newRecipientBalance);

                    return true;
                }
                else
                {
                    Runtime.Log("Transfer: Sender has insufficient balance.");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a host. 
        /// A host is a person/organization who can create events for their customers to make bookings on.
        /// </summary>
        private static bool CreateHost(byte[] hostAddress, string data)
        {
            string[] requiredData = new string[] {
                "name"
            };

            //TODO: Enable after CoZ dApps contest
            ////Validate host scripthash
            //if (!Runtime.CheckWitness(hostAddress))
            //{
            //    Runtime.Log("Check witness failed.");
            //    return false;
            //}

            //Validate input
            if (hostAddress == null || data == null)
            {
                Runtime.Log("CreateHost: One or more required parameter is not specified.");
                return false;
            }

            //Validate required data fields
            if (!HasKeysInData(data, requiredData))
            {
                Runtime.Log("CreateHost: One or more required data field is not specified in values parameter.");
                return false;
            }
            
            //Put host data in storage
            Storage.Put(Storage.CurrentContext, Key(PREFIX_HOST, hostAddress), data);

            //Create host account
            BigInteger balance = 0;
            Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, hostAddress), balance);
            
            Runtime.Log("CreateHost: Successful.");

            return true;
        }
        
        /// <summary>
        /// Creates a customer.
        /// A customer is a user on the host's website or app. Customers are people who would book tickets to events created by hosts.
        /// </summary>
        private static bool CreateCustomer(byte[] customerAddress, string data)
        {
            string[] requiredData = new string[] {
                "host_address",
                "ref_id"
            };
            
            //Validate input
            if (customerAddress == null || data == null)
            {
                Runtime.Log("CreateCustomer: One or more required parameter is not specified.");
                return false;
            }

            //Validate required data fields
            if (!HasKeysInData(data, requiredData))
            {
                Runtime.Log("CreateCustomer: One or more required data field is not specified in values parameter.");
                return false;
            }
            
            //Put customer data in storage
            Storage.Put(Storage.CurrentContext, Key(PREFIX_CUSTOMER, customerAddress), data);

            //Create customer account
            BigInteger balance = 0;
            Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, customerAddress), balance);

            Runtime.Log("CreateCustomer: Successfully created customer.");

            return true;
        }

        /// <summary>
        /// Creates an event.
        /// Events are created by host and put up for bookings. Customers of the host can make bookings on events.
        /// </summary>
        private static bool CreateEvent(byte[] eventAddress, string data)
        {
            string[] requiredData = new string[] {
                "host_address",
                "ref_id",
                "title",
                "start_date",
                "status"
            };

            //Validate input
            if (eventAddress == null || data == null)
            {
                Runtime.Log("CreateEvent: One or more required parameter is not specified.");
                return false;
            }

            //Validate required data fields
            if(!HasKeysInData(data, requiredData))
            {
                Runtime.Log("CreateEvent: One or more required data field is not specified in values parameter.");
                return false;
            }
            
            //Put event data in storage
            Storage.Put(Storage.CurrentContext, Key(PREFIX_EVENT, eventAddress), data);

            //Create event account
            //Note: Although an event is not a person but an entity created by the host,
            //      each event has it's own account for the purpose of collecting and refunding money during bookings/cancellations.
            //      Funds in the event's account are temporary held until the event start date is over, after which the host can claim all earnings via the ClaimEarnings operation
            BigInteger balance = 0;
            Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, eventAddress), balance);
            
            Runtime.Log("CreateEvent: Successfully created event.");

            return true;
        }

        /// <summary>
        /// Creates an event tier.
        /// A tier is a subcategory of an event with different pricing structure and availability.
        /// Example: A flight ticket would have the following tiers - Business Class, Preferred Seats, Economy
        /// </summary>
        private static bool CreateEventTier(byte[] tierAddress, string data)
        {
            string[] requiredData = new string[] {
                "host_address",
                "event_address",
                "ref_id",
                "title",
                "total_tickets",
                "available_tickets",
                "price"
            };

            //Validate input
            if (tierAddress == null || data == null)
            {
                Runtime.Log("CreateEventTier: One or more required parameter is not specified.");
                return false;
            }

            //Validate required data fields
            if (!HasKeysInData(data, requiredData))
            {
                Runtime.Log("CreateEventTier: One or more required data field is not specified in values parameter.");
                return false;
            }
            
            //Put tier data in storage
            string value = data;
            Storage.Put(Storage.CurrentContext, Key(PREFIX_EVENTTIER, tierAddress), value);
            
            Runtime.Log("CreateEventTier: Successfully created event tier.");

            return true;
        }

        /// <summary>
        /// Book an event for a customer and generates the tickets based on the given booking details.
        /// </summary>
        private static bool BookEventTier(byte[] customerAddress, byte[] eventAddress, byte[] tierAddress, string bookingDetails)
        {
            string[] requiredData = new string[] {
                "quantity",
                "ticket_numbers",
                "ticket_addresses"
            };

            //Validate input
            if (customerAddress == null || eventAddress == null || tierAddress == null || bookingDetails == null)
            {
                Runtime.Log("BookEvent: One or more required parameter is not specified.");
                return false;
            }

            //Validate required data fields
            if (!HasKeysInData(bookingDetails, requiredData))
            {
                Runtime.Log("BookEvent: One or more required data field is not specified in values parameter.");
                return false;
            }

            //Get ticket numbers array from parameter
            string[] ticketNumbers = Split(GetDataValue(bookingDetails, "ticket_numbers"), ';');

            //Get ticket addresses array from parameter
            string[] ticketAddresses = Split(GetDataValue(bookingDetails, "ticket_addresses"), ';');

            //Validate booking ticket quantity
            if (GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger() <= 0 ||
                GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger() != (BigInteger)ticketNumbers.Length ||
                (BigInteger)ticketNumbers.Length != (BigInteger)ticketAddresses.Length)
            {
                Runtime.Log("BookEvent: Invalid ticket quantiy, ticket numbers or ticket addresses.");
                return false;
            }

            //Get event
            byte[] eventData = Storage.Get(Storage.CurrentContext, Key(PREFIX_EVENT, eventAddress));
            if (eventData == null)
            {
                Runtime.Log("BookEvent: Event not found.");
                return false;
            }
            string eventDataString = eventData.AsString();

            //Get event tier
            byte[] eventTierData = Storage.Get(Storage.CurrentContext, Key(PREFIX_EVENTTIER, tierAddress));
            if (eventTierData == null)
            {
                Runtime.Log("BookEvent: Event tier not found.");
                return false;
            }
            string eventTierDataString = eventTierData.AsString();

            //TODO: Check if event address in storage matches the passed in from parameter (eventTierData["event_address"])

            //Validate event status
            if (GetDataValue(eventDataString, "status") != "Active")
            {
                Runtime.Log("BookEvent: Event is not active.");
                return false;
            }

            //Validate if event has already started
            if (CurrentTimestamp() > GetDataValue(eventDataString, "start_date").AsByteArray().AsBigInteger())
            {
                Runtime.Log("BookEvent: Event has already started.");
                return false;
            }

            //Validate if tickets are still available for the requested quantities
            if (GetDataValue(eventTierDataString, "available_tickets").AsByteArray().AsBigInteger() < GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger())
            {
                Runtime.Log("BookEvent: Not enough available tickets for the requested quantity.");
                return false;
            }

            //Calculate total booking cost
            BigInteger totalCost = GetDataValue(eventTierDataString, "price").AsByteArray().AsBigInteger() * GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger();

            //Check customer balance
            BigInteger customerBalance = BalanceOf(customerAddress);
            if (customerBalance < totalCost)
            {
                Runtime.Log("BookEvent: Customer has insufficient funds.");
                return false;
            }

            //Update available tickets in storage
            BigInteger remainingAvailableTickets = GetDataValue(eventTierDataString, "available_tickets").AsByteArray().AsBigInteger() - GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger();
            eventTierDataString = SetDataValue(eventTierDataString, "available_tickets", remainingAvailableTickets.AsByteArray().AsString());
            Storage.Put(Storage.CurrentContext, Key(PREFIX_EVENTTIER, tierAddress), eventTierDataString);

            //Transfer funds from customer account to event account
            Transfer(customerAddress, eventAddress, totalCost);

            //Generate tickets
            for (int i = 0; i < GetDataValue(bookingDetails, "quantity").AsByteArray().AsBigInteger(); i++)
            {
                string key = ticketAddresses[i];
                string ticketData = "";
                ticketData = AppendData(ticketData, "customer_address", customerAddress.AsString());
                ticketData = AppendData(ticketData, "event_address", eventAddress.AsString());
                ticketData = AppendData(ticketData, "tier_address", tierAddress.AsString());
                ticketData = AppendData(ticketData, "paid_price", GetDataValue(eventTierDataString, "price"));
                ticketData = AppendData(ticketData, "ticket_number", ticketNumbers[i]);
                ticketData = AppendData(ticketData, "status", "Active");

                Storage.Put(Storage.CurrentContext, Key(PREFIX_TICKET, key.AsByteArray()), ticketData);
            }

            Runtime.Log("BookEvent: Booking successful");

            return true;
        }

        /// <summary>
        /// Validates and redeems a ticket. Used tickets cannot be refunded.
        /// </summary>
        private static bool RedeemTicket(byte[] ticketAddress)
        {
            //Get ticket
            byte[] ticketData = Storage.Get(Storage.CurrentContext, Key(PREFIX_TICKET, ticketAddress).AsByteArray());
            if (ticketData == null)
            {
                Runtime.Log("RedeemTicket: Ticket not found.");
                return false;
            }
            string ticketDataString = ticketData.AsString();

            //Check if ticket is still active
            if (GetDataValue(ticketDataString, "status") != "Active")
            {
                Runtime.Log("RedeemTicket: Ticket is no longer active.");
                return false;
            }

            //Update ticket status
            string value = SetDataValue(ticketDataString, "status", "Used");
            Storage.Put(Storage.CurrentContext, Key(PREFIX_TICKET, ticketAddress), value);

            return true;
        }

        /// <summary>
        /// Cancels a ticket and refund the paid price from the event account to the customer account.
        /// </summary>
        private static bool CancelTicket(byte[] ticketAddress)
        {
            //Get ticket
            byte[] ticketData = Storage.Get(Storage.CurrentContext, Key(PREFIX_TICKET, ticketAddress));
            if (ticketData == null)
            {
                Runtime.Log("CancelTicket: Ticket not found.");
                return false;
            }
            string ticketDataString = ticketData.AsString();

            //Get event
            byte[] eventAddress = GetDataValue(ticketDataString, "event_address").AsByteArray();
            byte[] eventData = Storage.Get(Storage.CurrentContext, Key(PREFIX_EVENT, eventAddress));
            if (eventData == null)
            {
                Runtime.Log("CancelTicket: Event not found.");
                return false;
            }
            string eventDataString = eventData.AsString();

            //Get event tier
            byte[] tierAddress = GetDataValue(ticketData.AsString(), "tier_address").AsByteArray();
            byte[] eventTierData = Storage.Get(Storage.CurrentContext, Key(PREFIX_EVENTTIER, tierAddress));
            if (eventTierData == null)
            {
                Runtime.Log("CancelTicket: Event tier not found.");
                return false;
            }
            string eventTierDataString = eventTierData.AsString();

            //Check if ticket is still active
            if (GetDataValue(ticketDataString, "status") == "Used" || GetDataValue(ticketDataString, "status") == "Cancelled")
            {
                Runtime.Log("CancelTicket: Ticket has been used or cancelled.");
                return false;
            }

            //Update ticket status in storage
            string value = SetDataValue(ticketDataString, "status", "Cancelled");
            Storage.Put(Storage.CurrentContext, Key(PREFIX_TICKET, ticketAddress), value);

            //Refund ticket price to customer by transferring funds from event account to customer account
            Transfer(
                GetDataValue(ticketDataString, "event_address").AsByteArray(), 
                GetDataValue(ticketDataString, "customer_address").AsByteArray(), 
                GetDataValue(ticketDataString, "paid_price").AsByteArray().AsBigInteger());

            //Update available tickets in storage
            BigInteger newAvailableTickets = GetDataValue(eventTierDataString, "available_tickets").AsByteArray().AsBigInteger() + 1;
            eventTierDataString = SetDataValue(eventTierDataString, "available_tickets", newAvailableTickets.AsByteArray().AsString());
            Storage.Put(Storage.CurrentContext, Key(PREFIX_EVENTTIER, tierAddress), eventTierDataString);

            Runtime.Log("CancelTicket: Cancellation successful.");

            return true;
        }
        
        /// <summary>
        /// Claims the earnings of an event after the event start date is over.
        /// All funds from the event account is transferred to the host account.
        /// </summary>
        private static bool ClaimEarnings(byte[] eventAddress)
        {
            //Get event
            byte[] eventData = Storage.Get(Storage.CurrentContext, Key(PREFIX_EVENT, eventAddress));
            if (eventData == null)
            {
                Runtime.Log("BookEvent: Event not found.");
                return false;
            }
            string eventDataString = eventData.AsString();
            
            //Check if event status is not closed or cancelled
            if(GetDataValue(eventDataString, "status") == "Closed" || GetDataValue(eventDataString, "status") == "Cancelled")
            {
                Runtime.Log("BookEvent: Event is already claimed or cancelled.");
                return false;
            }

            //Transfer all funds from event account to host account
            byte[] hostAddress = GetDataValue(eventDataString, "host_address").AsByteArray();
            Transfer(eventAddress, hostAddress, BalanceOf(eventAddress));

            //Update event status in storage
            string value = SetDataValue(eventDataString, "status", "Closed");
            Storage.Put(Storage.CurrentContext, Key(PREFIX_EVENT, eventAddress), value);

            return true;
        }

        //Minting tokens is currently disabled until at a later stage of development.

        //// The function MintTokens is only usable by the chosen wallet
        //// contract to mint a number of tokens proportional to the
        //// amount of neo sent to the wallet contract. The function
        //// can only be called during the tokenswap period
        //// 将众筹的neo转化为等价的ico代币
        //public static bool MintTokens()
        //{
        //    byte[] sender = GetSender();
        //    // contribute asset is not neo
        //    if (sender.Length == 0)
        //    {
        //        return false;
        //    }
        //    ulong contribute_value = GetContributeValue();
        //    // the current exchange rate between ico tokens and neo during the token swap period
        //    // 获取众筹期间ico token和neo间的转化率
        //    ulong swap_rate = CurrentSwapRate();
        //    // crowdfunding failure
        //    // 众筹失败
        //    if (swap_rate == 0)
        //    {
        //        Refund(sender, contribute_value);
        //        return false;
        //    }
        //    // you can get current swap token amount
        //    ulong token = CurrentSwapToken(sender, contribute_value, swap_rate);
        //    if (token == 0)
        //    {
        //        return false;
        //    }
        //    // crowdfunding success
        //    // 众筹成功
        //    BigInteger balance = Storage.Get(Storage.CurrentContext, sender).AsBigInteger();
        //    Storage.Put(Storage.CurrentContext, sender, token + balance);
        //    BigInteger totalSupply = Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        //    Storage.Put(Storage.CurrentContext, "totalSupply", token + totalSupply);
        //    Transferred(null, sender, token);
        //    return true;
        //}

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>Current uint format date.</returns>
        private static BigInteger CurrentTimestamp()
        {
            uint height = Blockchain.GetHeight();
            Header header = Blockchain.GetHeader(height);
            uint res = header.Timestamp + 10;
            Runtime.Notify("NOW", res);
            return header.Timestamp + 10;
        }

        /// <summary>
        /// Concatinates a prefix string with an id.
        /// </summary>
        /// <param name="prefix">Prefix string.</param>
        /// <param name="id">Key Id.</param>
        /// <returns>String of the concatinated prefix with id.</returns>
        private static string Key(string prefix, byte[] id)
        {
            return string.Concat(prefix, id.AsString());
        }

        #endregion

        #region Collection Helpers
        
        // The following methods are substitute for the System.Collections library as it is not supported.
        // The purpose of using collections is to allow storing a KeyValue collection in a single storage key, 
        // allowing for structured data in storage.

        private static bool HasKeysInData(string data, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!HasKeyInData(data, keys[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private static bool HasKeyInData(string data, string key)
        {
            bool found = false;

            string[] keys = GetDataKeys(data);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == key)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        private static string AppendData(string data, string newKey, string newValue)
        {
            return data + "," + newKey + ":" + newValue;
        }

        private static string GetDataValue(string data, string key)
        {
            bool found = false;
            string result = null;

            string[] keys = GetDataKeys(data);
            string[] values = GetDataValues(data);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == key)
                {
                    result = values[i];
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Key " + key + " not found in data.");
            }
        }
        private static string SetDataValue(string data, string key, string value)
        {
            string[] keys = GetDataKeys(data);
            string[] values = GetDataValues(data);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == key)
                {
                    values[i] = value;
                    break;
                }
            }

            return Serialize(keys, values);
        }

        private static string[] GetDataKeys(string data)
        {
            string[] result = new string[100];
            int resultCount = 0;

            string[] items = Split(data, ',');
            for (int i = 0; i < items.Length; i++)
            {
                result[resultCount] = Split(items[i], ':')[0];
                resultCount++;
            }

            return FitArray(result);
        }
        private static string[] GetDataValues(string data)
        {
            string[] result = new string[100];
            int resultCount = 0;

            string[] items = Split(data, ',');
            for (int i = 0; i < items.Length; i++)
            {
                result[resultCount] = Split(items[i], ':')[1];
                resultCount++;
            }

            return FitArray(result);
        }

        public static string Serialize(string[] keys, string[] values)
        {
            string result = "";

            for (int i = 0; i < keys.Length; i++)
            {
                result += keys[i] + ":" + values[i];

                if (i < keys.Length - 1)
                {
                    result += ",";
                }
            }

            return result;
        }

        /// <summary>
        /// Resizes a larger array into a smaller one by removing null values.
        /// Substitute for List<> or Array.Resize as both is not supported.
        /// </summary>
        public static string[] FitArray(string[] value)
        {
            int length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == null)
                {
                    break;
                }
                else
                {
                    length++;
                }
            }
            string[] result = new string[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = value[i];
            }

            return result;
        }

        /// <summary>
        /// Substitute for the string.Split method as string.Split is not supported.
        /// </summary>
        public static string[] Split(string input, char delimiter)
        {
            string[] bufferArray = new string[100];
            string buffer = "";
            int bufferArrayCount = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == delimiter)
                {
                    bufferArray[bufferArrayCount] = buffer;
                    bufferArrayCount++;
                    buffer = "";
                }
                else
                {
                    buffer += input[i];
                }
            }
            bufferArray[bufferArrayCount] = buffer;
            bufferArrayCount++;

            //resize array
            int length = 0;
            for (int i = 0; i < bufferArray.Length; i++)
            {
                if (bufferArray[i] == null)
                {
                    break;
                }
                else
                {
                    length++;
                }
            }
            string[] result = new string[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bufferArray[i];
            }

            return result;
        }

        #endregion

    }
}

