using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetaExchangeSowaLabs.Entities;
using MetaExchangeSowaLabs.Enums;
using Newtonsoft.Json;

namespace MetaExchangeSowaLabs
{
    class Program
    {
        private const string _PathToOrdersFile = "order_books_data";
        private const string _PathToBalanceFile = "order_books_balance";
        private const int _NumberOfOrderBooks = 10;
        
        static void Main(string[] args)
        {
            //Extract order books and balance raw data from the file
            var orderBooks = ExtractNRows(_PathToOrdersFile, _NumberOfOrderBooks);
            var orderBooksUserBalance = ExtractNRows(_PathToBalanceFile, _NumberOfOrderBooks);
            
            
            //Call the algorithm 
            var result = MetaExchangeBestPrice(orderBooks,
                orderBooksUserBalance,
                TypeOfOrderEnum.Buy,
                (decimal) 1500);
            
            //Print the calculated optimal steps
            foreach (var (key, value) in result)
            {
                Console.WriteLine($"For cryptoexchange with id {key} do this: ");
                value.ForEach(Console.WriteLine);
            }
        }

        private static IEnumerable<string> ExtractNRows(string path, int numberOfRows)
        {
            return File.ReadLines(path).Take(numberOfRows);
        }
        

        private static Dictionary<string,List<string>> MetaExchangeBestPrice( IEnumerable<string> orderBooks, IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        {
            //Check if the amountOfBtc is valid
            if (amountOfBtc <= 0)
                throw new Exception("Illegal amount of BTC provided!");

            //Deserialize the entities
            var unorderedOrderBooks = DeserializeEntity<OrderBookEntity>(orderBooks);
            var userBalance =  DeserializeEntity<OrderBookBalanceEntity>(orderBooksUserBalance);
            
            
            //Some starting edge-cases
            if (!unorderedOrderBooks.Any())
            {
                throw new Exception("No order books available!");
            }
            if (!userBalance.Any())
            {
                throw new Exception("No user balance available!");
            }
            
           
            var result = typeOfOrder switch
            {
                TypeOfOrderEnum.Buy => HandleBuy(unorderedOrderBooks, userBalance, amountOfBtc),
                TypeOfOrderEnum.Sell => HandleSell(unorderedOrderBooks, userBalance, amountOfBtc),
                _ => throw new Exception("Illegal order!")
            };
            
            return result;
        }

        private static Dictionary<string,List<string>> HandleBuy(IEnumerable<OrderBookEntity> unorderedOrderBooks, List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if User has any money at all
            var userEurBalance = userBalance.Sum(balance => balance.Eur);
            if (userEurBalance == 0)
            {
                throw new Exception("User doesn't have any money!");
            }
            
            Console.WriteLine($"Starting EUR balance: {userEurBalance} EUR.");

            //Make a Dictionary from userBalance as we will be checking it constantly
            //remove the crypt-exchanges where the EUR balance is 0
            var userBalanceDict = userBalance
                .Where(x => x.Eur > 0)
                .ToDictionary(x => x.OrderBookId);
                
            
            //Order all of the Bids from all of the CryptoExchanges and only take the ones where user has some balance
            var metaExchange = OrderMetaExchangeByTypeOfOrder(unorderedOrderBooks, TypeOfOrderEnum.Buy)
                .Where(x => userBalanceDict.ContainsKey(x.OrderBookId)).ToList();
            
            
            var incrementalBoughtBtc = (decimal) 0;
            var incrementalPurchaseInEur  = (decimal) 0;
            var history = new Dictionary<int, decimal>();
            
            for (var i = 0; i < metaExchange.Count; i++)
            {
                var order = metaExchange[i];
                var userBalanceEntity= userBalanceDict[order.OrderBookId];

                //If the User balance has been used up there is no need to go further anymore
                if(incrementalPurchaseInEur == userEurBalance) break;
                
                //If user doesn't have EUR at the orderBook traverse over it or perhaps if it's possible to have a negative balance..
                if(userBalanceEntity.Eur <= 0) continue;
                
                //If we've already found the way to buy all the coins
                if(incrementalBoughtBtc == amountOfBtc) break;
                
                //How many BTC User can buy for the current price
                var howManyBtcUserCanBuy = userBalanceEntity.Eur / order.Price;
                
                //How many BTC User actually needs to buy
                var howManyBtcUserNeedsToBuy = Math.Min(amountOfBtc - incrementalBoughtBtc, howManyBtcUserCanBuy); 

                // User can buy the whole amount or partial
                var realBtcUserWillBuy = Math.Min(howManyBtcUserNeedsToBuy, order.Amount);
                //EUR needs to be rounded to two decimal places using banker's rounding.
                var costOfPurchase = Math.Round(realBtcUserWillBuy * order.Price,2, MidpointRounding.ToEven);
                incrementalBoughtBtc += realBtcUserWillBuy;
                incrementalPurchaseInEur += costOfPurchase;
                userBalanceEntity.Eur -= costOfPurchase;
                history.Add(i, realBtcUserWillBuy);
            }

            if (incrementalBoughtBtc != amountOfBtc) throw new Exception("Couldn't buy the desired BTC!");
            
            Console.WriteLine($"Can buy {amountOfBtc} BTC for {incrementalPurchaseInEur} EUR");
            Console.WriteLine($"Ending balance: {userEurBalance-incrementalPurchaseInEur} EUR.");
            Console.WriteLine("The steps are provided below: ");
            
            //Save the steps for every cryptoexchange separately  
            var transactions = new Dictionary<string, List<string>>();
            foreach (var (key, value) in history)
            {
                //We can't use order IDs as they are null...
                var transactionTemplate = $"Buy {value} BTC at {metaExchange[key].Price} EUR";

                if (transactions.ContainsKey(metaExchange[key].OrderBookId))
                {
                    transactions[metaExchange[key].OrderBookId].Add(transactionTemplate);
                    continue;
                }
                
                transactions.Add(metaExchange[key].OrderBookId, new List<string>{ transactionTemplate });
            }
            
            return transactions;
        }
        
        private static Dictionary<string, List<string>> HandleSell(IEnumerable<OrderBookEntity> unorderedOrderBooks, List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if Use has enough BTC to sell
            var userBtcBalance = userBalance.Sum(balance => balance.Bitcoin);
            if (userBtcBalance < amountOfBtc)
            {
                throw new Exception("User hasn't got enough BTC to sell!");
            }
            
            Console.WriteLine($"Starting BTC balance: {userBtcBalance} BTC.");

            //Make a Dictionary from userBalance as we will be checking it constantly
            //remove the crypt-exchanges where the Bitcoin balance is 0
            var userBalanceDict = userBalance
                .Where(x => x.Bitcoin > 0)
                .ToDictionary(x => x.OrderBookId);
            
            //Order all of the Bids from all of the CryptoExchanges and only take the ones where user has some balance
            var metaExchange = OrderMetaExchangeByTypeOfOrder(unorderedOrderBooks, TypeOfOrderEnum.Sell)
                .Where(x => userBalanceDict.ContainsKey(x.OrderBookId)).ToList();
            
            
            var incrementalSoldBtc = (decimal) 0;
            var incrementalSellingInEur = (decimal) 0;
            var history = new Dictionary<int, decimal>();
            for (var i = 0; i < metaExchange.Count; i++)
            {
                var order = metaExchange[i];
                var userBalanceEntity= userBalanceDict[order.OrderBookId];

                //If user doesn't have bitcoin at the orderBook traverse over it
                if(userBalanceEntity.Bitcoin ==  0) continue;
                
                //If we've already found the way to sell all the coins
                if(incrementalSoldBtc == amountOfBtc) break;
                
                //How many User actually needs to sell
                var howManyBtcUserNeedsToSell = Math.Min(amountOfBtc - incrementalSoldBtc, userBalanceEntity.Bitcoin); 

                // User can sell the whole amount or partial
                var realBtcUserWillSell = Math.Min(howManyBtcUserNeedsToSell, order.Amount);
                //EUR round to two digits using banker rounding
                incrementalSellingInEur += Math.Round(realBtcUserWillSell * order.Price, 2, MidpointRounding.ToEven);
                incrementalSoldBtc += realBtcUserWillSell;
                userBalanceEntity.Bitcoin -= realBtcUserWillSell;
                history.Add(i, realBtcUserWillSell);
            }

            if (incrementalSoldBtc != amountOfBtc) throw new Exception("Couldn't buy the desired BTC!");
            
            Console.WriteLine($"Can sell {amountOfBtc} BTC for {incrementalSellingInEur} EUR");
            Console.WriteLine($"Ending balance: {userBtcBalance - amountOfBtc} BTC.");
            Console.WriteLine("The steps are provided below: ");
            
            //Save the steps for every cryptoexchange separately  
            var transactions = new Dictionary<string, List<string>>();
            foreach (var (key, value) in history)
            {
                //We can't use order IDs as they are null...
                var transactionTemplate = $"Sell {value} BTC at {metaExchange[key].Price} EUR";

                if (transactions.ContainsKey(metaExchange[key].OrderBookId))
                {
                    transactions[metaExchange[key].OrderBookId].Add(transactionTemplate);
                    continue;
                }
                
                transactions.Add(metaExchange[key].OrderBookId, new List<string>{ transactionTemplate });
            }
            
            return transactions;
        }
        
        
        private static List<MetaExchangeOrderEntity> OrderMetaExchangeByTypeOfOrder(IEnumerable<OrderBookEntity> unorderedOrderBooks, TypeOfOrderEnum typeOfOrderEnum)
        {
            var orderedAsksOrBids = new List<MetaExchangeOrderEntity>();
            foreach (var orderBook in unorderedOrderBooks)
            {
                switch (typeOfOrderEnum)
                {
                    case TypeOfOrderEnum.Buy:
                        orderedAsksOrBids.AddRange(orderBook.Asks.Select(x => new MetaExchangeOrderEntity(x.Order,
                            orderBook.OrderBookId)));
                        break;
                    case TypeOfOrderEnum.Sell:
                        orderedAsksOrBids.AddRange(orderBook.Bids.Select(x => new MetaExchangeOrderEntity(x.Order,
                            orderBook.OrderBookId)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(typeOfOrderEnum), typeOfOrderEnum, "Illegal order");
                }
            }
            
            
            //sort by Price and Type
            return  typeOfOrderEnum == TypeOfOrderEnum.Buy 
                ? orderedAsksOrBids.OrderBy(x => x.Price).ToList()
                : orderedAsksOrBids.OrderByDescending(x => x.Price).ToList();
        }
        
        private static List<T> DeserializeEntity<T> (IEnumerable<string> jsonRows)
        {
            
            var deserializeEntities = new List<T>();
            foreach (var x in jsonRows)
            {
                //Basic check if the json is properly formatted
                var indexOfTab = x.IndexOf('\t');
                var indexOfCurlyBracer = x.IndexOf('{');
                if (indexOfTab == -1 || indexOfCurlyBracer == -1)
                    throw new Exception("Json not properly formatted!");
                

                //Get the timestamp as ID so we can connect the balance and the data
                var orderBookId = x.Remove(indexOfTab);
                
                //remove the timestamp so the row can be deserialized
                var cleanJson = x.Remove(0, indexOfCurlyBracer);
                
                //Deserialize to .Net classes and save the ID
                try
                {
                    dynamic deserializedEntity = JsonConvert.DeserializeObject<T>(cleanJson);
                    if (deserializedEntity != null) deserializedEntity.OrderBookId = orderBookId;
                    
                    deserializeEntities.Add((T)deserializedEntity);
                }
                catch (JsonException e)
                {
                    throw new Exception("Couldn't serialize the document! : " + e);
                }
            }

            return deserializeEntities;
        }
    }

}