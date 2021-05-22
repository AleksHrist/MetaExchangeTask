using System;

namespace MetaExchangeSowaLabs.Lib.CustomErrors
{
    public class IllegalAmountOfBtcException : Exception
    {
        public IllegalAmountOfBtcException(decimal amount)
            : base($"Illegal ammount of BTC : {amount}!")
        {

        }
    }
}