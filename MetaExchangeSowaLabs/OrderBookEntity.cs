using System;
using System.Collections.Generic;
using System.Transactions;

namespace MetaExchangeSowaLabs
{
    public class OrderBookBalanceEntity
    {
        public string OrderBookId { get; set; }
        public decimal Bitcoin { get; set; }
        public decimal Eur { get; set; }
    }

    
    public class OrderBookEntity
    {
        public string OrderBookId { get; set; }
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
    
    public class MetaExchangeOrderEntity : OrderEntity
    {
        public string OrderBookId { get; set; }
        
        public MetaExchangeOrderEntity(OrderEntity orderEntity, string orderBookId)
        {
            Id = orderEntity.Id;
            Time = orderEntity.Time;
            Type = orderEntity.Type;
            Kind = orderEntity.Kind;
            Amount = orderEntity.Amount;
            Price = orderEntity.Price;
            OrderBookId = orderBookId;
        }
    }
    
    public enum TypeOfOrderEnum
    {
        Buy,
        Sell
    }
}