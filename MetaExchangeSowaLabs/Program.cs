using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MetaExchangeSowaLabs
{
    class Program
    {
        private const string _PathToOrdersFile = "order_books_data";
        private const string _PathToBalanceFile = "order_books_balance";
        private const int _NumberOfOrderBooks = 5000;
        
        static void Main(string[] args)
        {
            //Extract order books raw data from the file
            var orderBooks = ExtractNRows(_PathToOrdersFile, _NumberOfOrderBooks);
            var orderBooksUserBalance = ExtractNRows(_PathToBalanceFile, _NumberOfOrderBooks);
            
            //Extract balance raw data from the file
            
            //Call the algorithm 
            var result = MetaExchangeBestPrice(orderBooks, orderBooksUserBalance, TypeOfOrderEnum.Buy, (decimal) 2.3139);
            
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
            Stopwatch sp = Stopwatch.StartNew();
            
            //Deserialize the entities
            var unorderedOrderBooks = DeserializeEntity<OrderBookEntity>(orderBooks);
            var metaExchangeBalance =  DeserializeEntity<OrderBookBalanceEntity>(orderBooksUserBalance);
            
            
            //Sort the Asks and Bids of the order for easier computation
            var metaExchange = OrderAsksAndBidsForOrderBooks(unorderedOrderBooks);


            return new List<string>();
        }
        

        private static List<OrderBookEntity> OrderAsksAndBidsForOrderBooks(IEnumerable<OrderBookEntity> unorderedOrderBooks)
        {
            var metaExchange = new List<OrderBookEntity>();

            //sort by Price
            foreach (var orderBook in unorderedOrderBooks)
            {
                var orderedAsks = orderBook.Asks.OrderBy(x => x.Order.Price).ToList();
                var orderedBids = orderBook.Bids.OrderByDescending(x => x.Order.Price).ToList();

                metaExchange.Add(new OrderBookEntity
                {
                    AcqTime = orderBook.AcqTime,
                    Asks = orderedAsks,
                    Bids = orderedBids
                });
            }

            return metaExchange;
        }

        private static IEnumerable<T> DeserializeEntity<T> (IEnumerable<string> jsonRows)
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
                    
                    deserializeEntities.Add(deserializedEntity);
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