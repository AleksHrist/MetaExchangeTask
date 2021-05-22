using System;

namespace MetaExchangeSowaLabs.Lib.CustomErrors
{
    public class EmptyOrderBookOrBalanceException : Exception
    {
        public EmptyOrderBookOrBalanceException(string type)
            : base($"No {type} available!")
        {

        }
    }
}