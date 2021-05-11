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
        private const int _NumberOfOrderBooks = 9;
        
        static void Main(string[] args)
        {
            //Extract order books and balance raw data from the file
            var orderBooks = ExtractNRows(_PathToOrdersFile, _NumberOfOrderBooks);
            var orderBooksUserBalance = ExtractNRows(_PathToBalanceFile, _NumberOfOrderBooks);
            
            
            //Call the algorithm 
            var result = MetaExchangeBestPrice(orderBooks, orderBooksUserBalance, TypeOfOrderEnum.Buy, (decimal) 11.111111111111111);
            
            //Print the calculated optimal steps
            result.ForEach(Console.WriteLine);
        }

        private static IEnumerable<string> ExtractNRows(string path, int numberOfRows)
        {
            return File.ReadLines(path).Take(numberOfRows);
        }
        

        private static List<string> MetaExchangeBestPrice( IEnumerable<string> orderBooks, IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        {
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

        private static List<string> HandleBuy(IEnumerable<OrderBookEntity> unorderedOrderBooks, List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if we have any money at all
            var userEurBalance = userBalance.Sum(balance => balance.Eur);
            if (userEurBalance == 0)
            {
                throw new Exception("User doesn't have any money!");
            }

            //Make a Dictionary from userBalance as we will be checking it constantly
            var userBalanceDict = userBalance.ToDictionary(x => x.OrderBookId);
            
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
                
                //If user doesn't have EUR at the orderBook traverse over it
                if(userBalanceEntity.Eur ==  0) continue;
                
                //If we've already found the way to buy all the coins
                if(incrementalBoughtBtc == amountOfBtc) break;
                
                //How many BTC User can buy for the current price
                var howManyBtcUserCanBuy = userBalanceEntity.Eur / order.Price;
                
                //How many BTC User actually needs to buy
                var howManyBtcUserNeedsToBuy = Math.Min(amountOfBtc - incrementalBoughtBtc, howManyBtcUserCanBuy); 

                // User can buy the whole amount or partial
                var realBtcUserWillBuy = Math.Min(howManyBtcUserNeedsToBuy, order.Amount);
                var costOfPurchase = realBtcUserWillBuy * order.Price;
                incrementalBoughtBtc += realBtcUserWillBuy;
                incrementalPurchaseInEur += costOfPurchase;
                userBalanceEntity.Eur -= costOfPurchase;
                history.Add(i, realBtcUserWillBuy);
            }

            if (incrementalBoughtBtc != amountOfBtc) throw new Exception("Couldn't buy the desired BTC!");

            var transactions = new List<string>();
            foreach (var (key, value) in history)
            {
                //We can't use order IDs as they are null...
                var transactionTemplate =
                    $"On cryptoexchange with ID: {metaExchange[key].OrderBookId} " +
                    $"buy {value} BTC where each costs: {metaExchange[key].Price} EUR";
                
                transactions.Add(transactionTemplate);
            }
            
            return transactions;

          
        }
        
        private static List<string> HandleSell(IEnumerable<OrderBookEntity> unorderedOrderBooks, List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if we have enough BTC to sell
            var userBtcBalance = userBalance.Sum(balance => balance.Bitcoin);
            if (userBtcBalance < amountOfBtc)
            {
                throw new Exception("User hasn't got enough BTC to sell!");
            }

            //Make a Dictionary from userBalance as we will be checking it constantly
            var userBalanceDict = userBalance.ToDictionary(x => x.OrderBookId);
            
            //Order all of the Bids from all of the CryptoExchanges and only take the ones where user has some balance
            var metaExchange = OrderMetaExchangeByTypeOfOrder(unorderedOrderBooks, TypeOfOrderEnum.Sell)
                .Where(x => userBalanceDict.ContainsKey(x.OrderBookId)).ToList();
            
            
            var incrementalSoldBtc = (decimal) 0;
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
                var costOfPurchase = realBtcUserWillSell * order.Price;
                incrementalSoldBtc += realBtcUserWillSell;
                userBalanceEntity.Bitcoin -= realBtcUserWillSell;
                history.Add(i, realBtcUserWillSell);
            }

            if (incrementalSoldBtc != amountOfBtc) throw new Exception("Couldn't buy the desired BTC!");

            var transactions = new List<string>();
            foreach (var (key, value) in history)
            {
                //We can't use order IDs as they are null...
                var transactionTemplate =
                    $"On cryptoexchange with ID: {metaExchange[key].OrderBookId} " +
                    $"sell {value} BTC where each costs: {metaExchange[key].Price} EUR";
                
                transactions.Add(transactionTemplate);
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
                //Get the timestamp as ID so we can connect the balance and the data
                var orderBookId = x.Remove(x.IndexOf('\t'));
                
                //remove the timestamp so the row can be deserialized
                var cleanJson = x.Remove(0, x.IndexOf('{'));
                
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