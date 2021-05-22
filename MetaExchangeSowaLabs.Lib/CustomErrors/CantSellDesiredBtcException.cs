using System;

namespace MetaExchangeSowaLabs.Lib.CustomErrors
{
    public class CantSellDesiredBtcException : Exception
    {
        public  CantSellDesiredBtcException(decimal amount) 
            : base($"Can't sell {amount} BTC!")
        {
        }
        
    }
}