using System;

namespace MetaExchangeSowaLabs.CustomErrors
{
    public class CantSellDesiredBtcException : Exception
    {
        public  CantSellDesiredBtcException(decimal amount) 
            : base($"Can't sell {amount} BTC!")
        {
        }
        
    }
}