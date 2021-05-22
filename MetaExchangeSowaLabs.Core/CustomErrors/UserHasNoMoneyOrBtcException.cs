using System;

namespace MetaExchangeSowaLabs.Core.CustomErrors
{
    public class UserHasNoMoneyOrBtcException : Exception
    {
        public UserHasNoMoneyOrBtcException(string type)
            : base($"User doesn't have enough {type}!")
        {

        }
    }
}