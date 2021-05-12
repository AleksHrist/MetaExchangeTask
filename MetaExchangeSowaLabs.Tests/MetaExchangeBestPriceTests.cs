using System;
using System.Collections.Generic;
using MetaExchangeSowaLabs.Enums;
using Xunit;

namespace MetaExchangeSowaLabs.Tests
{
    public class MetaExchangeBestPriceTests
    {

        [Fact]
        public void SendingEmptyListOrderBooksShouldThrowException()
        {
            var orderBooks = new List<string>();
            var orderBooksUserBalance = new List<string>{@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;
            
            Assert.Throws<Exception>(() => Program.MetaExchangeBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }
        
        [Fact]
        public void SendingEmptyListOrderBooksUserBalanceShouldThrowException()
        {
            var orderBooks = new List<string>{@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var orderBooksUserBalance = new List<string>();
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;
        
            Assert.Throws<Exception>(() => Program.MetaExchangeBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }
        
        // [Theory]
        // [MemberData(nameof(TestData))]
        // public void BuyingBitcoinsShouldProvideCorrectStepsTheory(Dictionary<string, List<string>> expectedResult, IEnumerable<string> orderBooks, IEnumerable<string> orderBooksUserBalance,
        //     TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        // {
        //     
        //     Assert.Equal(expectedResult,_sut.MetaExchangeBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        // }
        //
        // public static IEnumerable<object[]> TestData()
        // {
        //     yield return new object[]
        //     {
        //         new Dictionary<string, List<string>>()
        //     };
        // }
    }
}