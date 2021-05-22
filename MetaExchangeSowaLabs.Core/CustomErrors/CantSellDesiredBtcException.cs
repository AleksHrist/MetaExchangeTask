using System;

namespace MetaExchangeSowaLabs.Core.CustomErrors
{
    public class CantSellDesiredBtcException : Exception
    {
        public  CantSellDesiredBtcException(decimal amount) 
            : base($"Can't sell {amount} BTC!")
        {
        }
        
    }
}