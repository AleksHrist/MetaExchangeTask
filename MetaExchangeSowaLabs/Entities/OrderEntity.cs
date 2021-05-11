using System;
using MetaExchangeSowaLabs.Enums;
namespace MetaExchangeSowaLabs.Entities
{
    public class OrderEntity
    {
        public Guid? Id { get; set; }
        public DateTime Time { get; set; }
        public TypeOfOrderEnum Type { get; set; }
        public String Kind { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Price { get; set; }
    }
}