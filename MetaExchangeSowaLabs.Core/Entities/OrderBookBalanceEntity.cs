namespace MetaExchangeSowaLabs.Core.Entities
{
    public class OrderBookBalanceEntity
    {
        public string OrderBookId { get; set; }
        public decimal Bitcoin { get; set; }
        public decimal Eur { get; set; }
    }
}