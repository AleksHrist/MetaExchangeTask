using System.Collections.Generic;
using MetaExchangeSowaLabs.Core.CustomErrors;
using MetaExchangeSowaLabs.Core.Enums;
using MetaExchangeSowaLabs.Services.Interfaces;
using MetaExchangeSowaLabs.Services.Services;
using Newtonsoft.Json;
using Xunit;

namespace MetaExchangeSowaLabs.Tests
{
    public class MetaExchangeBestPriceTests
    {
        private readonly IMetaExchangeService _metaExchangeService;

        public MetaExchangeBestPriceTests()
        {
            _metaExchangeService = new MetaExchangeService();
        }

        [Fact]
        public void SendingEmptyListOrderBooksShouldThrowException()
        {
            var orderBooks = new List<string>();
            var orderBooksUserBalance = new List<string> {@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;

            
            Assert.Throws<EmptyOrderBookOrBalanceException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder,
                    amountOfBtc));
        }

        [Fact]
        public void SendingEmptyListOrderBooksUserBalanceShouldThrowException()
        {
            var orderBooks = new List<string> {@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var orderBooksUserBalance = new List<string>();
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;

            Assert.Throws<EmptyOrderBookOrBalanceException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder,
                    amountOfBtc));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void SendingNegativeOrZeroBtcAmountShouldThrowException(decimal amountOfBtc)
        {
            var orderBooks = new List<string> {@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var orderBooksUserBalance = new List<string> {@"1548759600.25189	{""Bitcoin"": 5, ""Eur"": 1221312301.12}"};
            var typeOfOrder = TypeOfOrderEnum.Buy;

            Assert.Throws<IllegalAmountOfBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void BadlyFormatedJsonShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}}
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
            };
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;

            Assert.Throws<JsonException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void UserWithNoMoneyWhenBuyingBtcShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 0}",
            };
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 15;

            Assert.Throws<UserHasNoMoneyOrBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void UserWithNoBtcWhenSellingBtcShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 0, ""Eur"": 123123}",
            };
            var typeOfOrder = TypeOfOrderEnum.Sell;
            var amountOfBtc = (decimal) 15;

            Assert.Throws<UserHasNoMoneyOrBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }


        [Theory]
        [MemberData(nameof(TestData))]
        public void BuyingOrSellingBitcoinsShouldProvideCorrectStepsTheory(
            Dictionary<string, List<string>> expectedResult, IEnumerable<string> orderBooks,
            IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc)
        {
            Assert.Equal(expectedResult,
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Buy 0.405 BTC at 2980.61 EUR",
                            "Buy 0.405 BTC at 2980.62 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Buy,
                (decimal) 0.810
            };

            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Sell 0.23 BTC at 2960.64 EUR",
                            "Sell 0.2323 BTC at 2960.63 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }",
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Sell,
                (decimal) 0.4623
            };

            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Sell 0.23 BTC at 2960.64 EUR",
                            "Sell 0.2323 BTC at 2960.63 EUR",
                            "Sell 0.23 BTC at 2960.62 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }",
                    @"1548759600.25190	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }"
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Sell,
                (decimal) 0.6923
            };
            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Sell 0.23 BTC at 2960.64 EUR",
                            "Sell 0.2323 BTC at 2960.63 EUR"
                        }
                    },
                    {
                        "1548759600.25190", new List<string>
                        {
                            "Sell 0.23 BTC at 2960.64 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }",
                    @"1548759600.25190	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }"
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                    @"1548759600.25190	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Sell,
                (decimal) 0.6923
            };

            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Buy 0.2323 BTC at 2960.61 EUR",
                            "Buy 0.23 BTC at 2960.62 EUR",
                            "Buy 0.23 BTC at 2960.63 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }",
                    @"1548759600.25190	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }"
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Buy,
                (decimal) 0.6923
            };

            yield return new object[]
            {
                new Dictionary<string, List<string>>
                {
                    {
                        "1548759600.25189", new List<string>
                        {
                            "Buy 0.2323 BTC at 2960.61 EUR",
                            "Buy 0.2277 BTC at 2960.62 EUR",
                        }
                    },
                    {
                        "1548759600.25190", new List<string>
                        {
                            "Buy 0.2323 BTC at 2960.61 EUR"
                        }
                    }
                },
                new List<string>
                {
                    @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }",
                    @"1548759600.25190	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.23,""Price"":2960.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.2323,""Price"":2960.61}}],
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}}] 
                    }"
                },
                new List<string>
                {
                    @"1548759600.25189	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                    @"1548759600.25190	{""Bitcoin"": 1, ""Eur"": 12301.12}",
                },
                TypeOfOrderEnum.Buy,
                (decimal) 0.6923
            };
        }


        [Fact]
        public void UserWithNotEnoughBtcWhenSellingBtcShouldThrowException()
        {
            IMetaExchangeService metaExchangeService = new MetaExchangeService();
            
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 14.123, ""Eur"": 123123}",
            };
            var typeOfOrder = TypeOfOrderEnum.Sell;
            var amountOfBtc = (decimal) 14.222;

            Assert.Throws<UserHasNoMoneyOrBtcException>(() =>
                metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void UserWithNotEnoughMoneyWhenBuyingBtcShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 14.123, ""Eur"": 1234.5}",
            };
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 0.5;

            Assert.Throws<CantBuyDesiredBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void UserWantsToBuyMoreThanAvailableBtcInTheMarketShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 14.123, ""Eur"": 1123123234.5}",
            };
            var typeOfOrder = TypeOfOrderEnum.Buy;
            var amountOfBtc = (decimal) 100;

            Assert.Throws<CantBuyDesiredBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }

        [Fact]
        public void UserWantsToSellMoreBtcThanOthersWantToBuyInTheMarketShouldThrowException()
        {
            var orderBooks = new List<string>
            {
                @"1548759600.25189	{
                    ""AcqTime"":""2019-01-29T11:00:00.2518854Z"",
                    ""Bids"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Buy"",""Kind"":""Limit"",""Amount"":0.01,""Price"":2960.63}}],
                    ""Asks"":[{""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.64}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.63}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.62}},
                              {""Order"":{""Id"":null,""Time"":""0001-01-01T00:00:00"",""Type"":""Sell"",""Kind"":""Limit"",""Amount"":0.405,""Price"":2980.61}}] 
                    }",
            };
            var orderBooksUserBalance = new List<string>
            {
                @"1548759600.25189	{""Bitcoin"": 100, ""Eur"": 1123123234.5}",
            };
            var typeOfOrder = TypeOfOrderEnum.Sell;
            var amountOfBtc = (decimal) 100;

            Assert.Throws<CantSellDesiredBtcException>(() =>
                _metaExchangeService.GetOptimalStepsForBestPrice(orderBooks, orderBooksUserBalance, typeOfOrder, amountOfBtc));
        }
    }
}