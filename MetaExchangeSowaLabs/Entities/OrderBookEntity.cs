using System;
using System.Collections.Generic;

namespace MetaExchangeSowaLabs.Entities
{
    public class OrderBookEntity
    {
        public string OrderBookId { get; set; }
        public DateTime AcqTime { get; set; }
        public List<BidAskEntity> Bids { get; set; }
        public List<BidAskEntity> Asks { get; set; }
    }
}