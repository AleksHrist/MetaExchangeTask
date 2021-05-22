using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetaExchangeSowaLabs.Lib.Enums;
using MetaExchangeSowaLabs.Lib.Helpers;
using MetaExchangeSowaLabs.Lib.Services;
using MetaExchangeSowaLabs.Lib.Services.Interfaces;

namespace MetaExchangeSowaLabs
{
    public class Program
    {
        private const string _PathToOrdersFile = "order_books_data";
        private const string _PathToBalanceFile = "order_books_balance";
        private const int _NumberOfOrderBooks = 10;
        
        static void Main(string[] args)
        {
            //Extract order books and balance raw data from the file
            var orderBooks = FileIOHelper.ExtractNRows(_PathToOrdersFile, _NumberOfOrderBooks);
            var orderBooksUserBalance = FileIOHelper.ExtractNRows(_PathToBalanceFile, _NumberOfOrderBooks);
            
            //Call the algorithm 
            IMetaExchangeService metaExchangeService = new MetaExchangeService();
            var result = metaExchangeService.GetOptimalStepsForBestPrice(orderBooks,
                orderBooksUserBalance,
                TypeOfOrderEnum.Sell,
                (decimal) 0.6923);
            
            //Print the calculated optimal steps
            metaExchangeService.PrintOptimalStepsToStdOut(result);
        }
    }

}