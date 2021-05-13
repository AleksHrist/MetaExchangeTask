using System;

namespace MetaExchangeSowaLabs.CustomErrors
{
    public class EmptyOrderBookOrBalanceException : Exception
    {
        public EmptyOrderBookOrBalanceException(string type)
            : base($"No {type} available!")
        {

        }
    }
}