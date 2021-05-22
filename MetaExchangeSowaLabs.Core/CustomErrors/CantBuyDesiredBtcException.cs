using System;

namespace MetaExchangeSowaLabs.Core.CustomErrors
{
    public class CantBuyDesiredBtcException : Exception
    {
        public  CantBuyDesiredBtcException(decimal amount) 
            : base($"Can't buy {amount} BTC!")
        {
        }
    }
}