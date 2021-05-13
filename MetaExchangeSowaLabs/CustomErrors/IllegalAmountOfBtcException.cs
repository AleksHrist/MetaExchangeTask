using System;

namespace MetaExchangeSowaLabs.CustomErrors
{
    public class IllegalAmountOfBtcException : Exception
    {
        public IllegalAmountOfBtcException(decimal amount)
            : base($"Illegal ammount of BTC : {amount}!")
        {

        }
    }
}