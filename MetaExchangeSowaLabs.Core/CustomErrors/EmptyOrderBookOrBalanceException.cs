using System;

namespace MetaExchangeSowaLabs.Core.CustomErrors
{
    public class EmptyOrderBookOrBalanceException : Exception
    {
        public EmptyOrderBookOrBalanceException(string type)
            : base($"No {type} available!")
        {

        }
    }
}