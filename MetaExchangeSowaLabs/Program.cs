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
        private const string _PathToFile = "order_books_data";
        private const int _NumberOfRows = 5000;
        
        static void Main(string[] args)
        {
            //Extract raw data from the file
            var orderBooks = ExtractNOrderBooks(_PathToFile, _NumberOfRows);
            
            //Call the algorithm 
            var result = MetaExchangeBestPrice(orderBooks, TypeOfOrderEnum.Buy, (decimal) 2.3139);
            
            //Print the calculated optimal steps
            result.ForEach(Console.WriteLine);
        }

        private static List<string> ExtractNOrderBooks(string path, int numberOfRows)
        {
            return File.ReadLines(path).Take(numberOfRows).ToList();
        }

        private static List<string> MetaExchangeBestPrice(List<string> orderBooks, TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        {
            var metaExchange = DeserializeOrderBooks(orderBooks);


            return new List<string>();
        }

        private static List<OrderBookEntity> DeserializeOrderBooks(List<string> orderBooks)
        {
            //remove the timestamp so the row can be deserialized
            var cleanOrderBooks = new List<string>();
            orderBooks.ForEach(x => { cleanOrderBooks.Add(x.Remove(0, x.IndexOf('{'))); });

            //Serialize the Order books so we can pass it as a parameter for deserialization
            var customSerializedOrderBooks = "[" + string.Join(",", cleanOrderBooks) + "]";

            //Deserialize to .Net classes
            List<OrderBookEntity> metaExchange;
            try
            {
                metaExchange = JsonConvert.DeserializeObject<List<OrderBookEntity>>(customSerializedOrderBooks);
            }
            catch (JsonException e)
            {
                throw new Exception("Couldn't serialize the document! : " + e);
            }

            return metaExchange;
        }
    }

}