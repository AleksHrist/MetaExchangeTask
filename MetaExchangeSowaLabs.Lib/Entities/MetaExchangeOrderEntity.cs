namespace MetaExchangeSowaLabs.Lib.Entities
{
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
}