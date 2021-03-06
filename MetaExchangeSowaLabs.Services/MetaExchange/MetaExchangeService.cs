using System;
using System.Collections.Generic;
using System.Linq;
using MetaExchangeSowaLabs.Contracts.Interfaces;
using MetaExchangeSowaLabs.Core.CustomErrors;
using MetaExchangeSowaLabs.Core.Entities;
using MetaExchangeSowaLabs.Core.Enums;
using MetaExchangeSowaLabs.Core.Json;

namespace MetaExchangeSowaLabs.Services.MetaExchange
{
    public class MetaExchangeService : IMetaExchangeService
    {
        public Dictionary<string, List<string>> GetOptimalStepsForBestPrice(IEnumerable<string> orderBooks, IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        {
            //Check if the amountOfBtc is valid
            if (amountOfBtc <= 0)
                throw new IllegalAmountOfBtcException(amountOfBtc);

                //Deserialize the entities
            var unorderedOrderBooks = Json.DeserializeEntity<OrderBookEntity>(orderBooks);
            var userBalance = Json.DeserializeEntity<OrderBookBalanceEntity>(orderBooksUserBalance);


            //Some starting edge-cases
            if (!unorderedOrderBooks.Any())
                throw new EmptyOrderBookOrBalanceException("order books");

            if (!userBalance.Any())
                throw new EmptyOrderBookOrBalanceException("user balance");


            Dictionary<string, List<string>> result;
            switch (typeOfOrder)
            {
                case TypeOfOrderEnum.Buy:
                    result = HandleBuy(unorderedOrderBooks, userBalance, amountOfBtc);
                    break;
                case TypeOfOrderEnum.Sell:
                    result = HandleSell(unorderedOrderBooks, userBalance, amountOfBtc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeOfOrder));
            }

            return result;
        }

        private Dictionary<string, List<string>> HandleBuy(IEnumerable<OrderBookEntity> unorderedOrderBooks,
            List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if User has any money at all
            decimal userEurBalance = userBalance.Sum(balance => balance.Eur);
            if (userEurBalance == 0)
                throw new UserHasNoMoneyOrBtcException("Eur");

            Console.WriteLine($"Starting EUR balance: {userEurBalance} EUR.");

            //Make a Dictionary from userBalance as we will be checking it constantly
            //remove the crypt-exchanges where the EUR balance is 0
            var userBalanceDict = userBalance
                .Where(x => x.Eur > 0)
                .ToDictionary(x => x.OrderBookId);


            //Order all of the Bids from all of the CryptoExchanges and only take the ones where user has some balance
            var metaExchange = OrderMetaExchangeByTypeOfOrder(unorderedOrderBooks, TypeOfOrderEnum.Buy)
                .Where(x => userBalanceDict.ContainsKey(x.OrderBookId)).ToList();


            decimal incrementalBoughtBtc = 0;
            decimal incrementalPurchaseInEur = 0;
            var history = new Dictionary<int, decimal>();

            for (int i = 0; i < metaExchange.Count; i++)
            {
                var order = metaExchange[i];
                var userBalanceEntity = userBalanceDict[order.OrderBookId];

                //If the User balance has been used up there is no need to go further anymore
                if (incrementalPurchaseInEur == userEurBalance) 
                    break;

                //If user doesn't have EUR at the orderBook traverse over it or perhaps if it's possible to have a negative balance..
                if (userBalanceEntity.Eur <= 0) 
                    continue;

                //If we've already found the way to buy all the coins
                if (incrementalBoughtBtc == amountOfBtc) 
                    break;

                //How many BTC User can buy for the current price
                decimal howManyBtcUserCanBuy = userBalanceEntity.Eur / order.Price;

                //How many BTC User actually needs to buy
                decimal howManyBtcUserNeedsToBuy = Math.Min(amountOfBtc - incrementalBoughtBtc, howManyBtcUserCanBuy);

                // User can buy the whole amount or partial
                decimal realBtcUserWillBuy = Math.Min(howManyBtcUserNeedsToBuy, order.Amount);
                //EUR needs to be rounded to two decimal places using banker's rounding.
                decimal costOfPurchase = Math.Round(realBtcUserWillBuy * order.Price, 2, MidpointRounding.ToEven);
                incrementalBoughtBtc += realBtcUserWillBuy;
                incrementalPurchaseInEur += costOfPurchase;
                userBalanceEntity.Eur -= costOfPurchase;
                history.Add(i, realBtcUserWillBuy);
            }

            if (incrementalBoughtBtc != amountOfBtc)
                throw new CantBuyDesiredBtcException(amountOfBtc);

            Console.WriteLine($"Can buy {amountOfBtc} BTC for {incrementalPurchaseInEur} EUR");
            Console.WriteLine($"Ending balance: {userEurBalance - incrementalPurchaseInEur} EUR.");
            Console.WriteLine("The steps are provided below: ");

            //Save the steps for every cryptoexchange separately  
            var transactions = SaveOptimalStepsInDictionary(history, metaExchange, TypeOfOrderEnum.Buy);

            return transactions;
        }

        private Dictionary<string, List<string>> HandleSell(IEnumerable<OrderBookEntity> unorderedOrderBooks,
            List<OrderBookBalanceEntity> userBalance, decimal amountOfBtc)
        {
            //Check if Use has enough BTC to sell
            decimal userBtcBalance = userBalance.Sum(balance => balance.Bitcoin);
            if (userBtcBalance < amountOfBtc)
                throw new UserHasNoMoneyOrBtcException("BTC");

            Console.WriteLine($"Starting BTC balance: {userBtcBalance} BTC.");

            //Make a Dictionary from userBalance as we will be checking it constantly
            //remove the crypt-exchanges where the Bitcoin balance is 0
            var userBalanceDict = userBalance
                .Where(x => x.Bitcoin > 0)
                .ToDictionary(x => x.OrderBookId);

            //Order all of the Bids from all of the CryptoExchanges and only take the ones where user has some balance
            var metaExchange = OrderMetaExchangeByTypeOfOrder(unorderedOrderBooks, TypeOfOrderEnum.Sell)
                .Where(x => userBalanceDict.ContainsKey(x.OrderBookId)).ToList();


            decimal incrementalSoldBtc =  0;
            decimal incrementalSellingInEur =  0;
            var history = new Dictionary<int, decimal>();
            for (int i = 0; i < metaExchange.Count; i++)
            {
                var order = metaExchange[i];
                var userBalanceEntity = userBalanceDict[order.OrderBookId];

                //If user doesn't have bitcoin at the orderBook traverse over it
                if (userBalanceEntity.Bitcoin == 0) 
                    continue;

                //If we've already found the way to sell all the coins
                if (incrementalSoldBtc == amountOfBtc) 
                    break;

                //How many User actually needs to sell
                decimal howManyBtcUserNeedsToSell = Math.Min(amountOfBtc - incrementalSoldBtc, userBalanceEntity.Bitcoin);

                // User can sell the whole amount or partial
                decimal realBtcUserWillSell = Math.Min(howManyBtcUserNeedsToSell, order.Amount);
                //EUR round to two digits using banker rounding
                incrementalSellingInEur += Math.Round(realBtcUserWillSell * order.Price, 2, MidpointRounding.ToEven);
                incrementalSoldBtc += realBtcUserWillSell;
                userBalanceEntity.Bitcoin -= realBtcUserWillSell;
                history.Add(i, realBtcUserWillSell);
            }

            if (incrementalSoldBtc != amountOfBtc) 
                throw new CantSellDesiredBtcException(amountOfBtc);

            Console.WriteLine($"Can sell {amountOfBtc} BTC for {incrementalSellingInEur} EUR");
            Console.WriteLine($"Ending balance: {userBtcBalance - amountOfBtc} BTC.");
            Console.WriteLine("The steps are provided below: ");

            //Save the steps for every cryptoexchange separately  
            var transactions = SaveOptimalStepsInDictionary(history, metaExchange, TypeOfOrderEnum.Sell);

            return transactions;
        }


        private IEnumerable<MetaExchangeOrderEntity> OrderMetaExchangeByTypeOfOrder(
            IEnumerable<OrderBookEntity> unorderedOrderBooks, TypeOfOrderEnum typeOfOrderEnum)
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
                        throw new ArgumentOutOfRangeException(nameof(typeOfOrderEnum), typeOfOrderEnum,
                            "Illegal order");
                }
            }


            //sort by Price and Type
            return typeOfOrderEnum == TypeOfOrderEnum.Buy
                ? orderedAsksOrBids.OrderBy(x => x.Price).ToList()
                : orderedAsksOrBids.OrderByDescending(x => x.Price).ToList();
        }

        private Dictionary<string, List<string>> SaveOptimalStepsInDictionary(Dictionary<int, decimal> history,
            List<MetaExchangeOrderEntity> metaExchange, TypeOfOrderEnum typeOfOrderEnum)
        {
            var transactions = new Dictionary<string, List<string>>();
            foreach (var (key, value) in history)
            {
                //We can't use order IDs as they are null...
                var transactionTemplate = $"{typeOfOrderEnum} {value:G0} BTC at {metaExchange[key].Price} EUR";

                if (transactions.ContainsKey(metaExchange[key].OrderBookId))
                {
                    transactions[metaExchange[key].OrderBookId].Add(transactionTemplate);
                    continue;
                }

                transactions.Add(metaExchange[key].OrderBookId, new List<string> {transactionTemplate});
            }

            return transactions;
        }
    }
}