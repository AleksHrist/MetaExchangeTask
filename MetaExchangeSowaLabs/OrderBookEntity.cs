using System;
using System.Collections.Generic;

namespace MetaExchangeSowaLabs
{
    public class OrderBookEntity
    {
        public DateTime AcqTime { get; set; }
        public List<BidAskEntity> Bids { get; set; }
        public List<BidAskEntity> Asks { get; set; }
    }

    public class BidAskEntity
    {
        public OrderEntity Order { get; set; }
    }

    public class OrderEntity
    {
        public Guid? Id { get; set; }
        public DateTime Time { get; set; }
        public TypeOfOrderEnum Type { get; set; }
        public String Kind { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Price { get; set; }
    }
    
    public enum TypeOfOrderEnum
    {
        Buy,
        Sell
    }
}