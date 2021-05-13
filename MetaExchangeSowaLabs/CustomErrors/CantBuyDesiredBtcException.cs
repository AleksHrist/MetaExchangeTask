using System;

namespace MetaExchangeSowaLabs.CustomErrors
{
    public class CantBuyDesiredBtcException : Exception
    {
        public  CantBuyDesiredBtcException(decimal amount) 
            : base($"Can't buy {amount} BTC!")
        {
        }
    }
}