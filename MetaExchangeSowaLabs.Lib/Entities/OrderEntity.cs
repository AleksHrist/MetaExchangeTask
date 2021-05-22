using System;
using MetaExchangeSowaLabs.Lib.Enums;

namespace MetaExchangeSowaLabs.Lib.Entities
{
    public class OrderEntity
    {
        public Guid? Id { get; set; }
        public DateTime Time { get; set; }
        public TypeOfOrderEnum Type { get; set; }
        public string Kind { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}