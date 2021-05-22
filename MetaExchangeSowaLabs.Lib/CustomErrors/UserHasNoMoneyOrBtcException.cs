using System;

namespace MetaExchangeSowaLabs.Lib.CustomErrors
{
    public class UserHasNoMoneyOrBtcException : Exception
    {
        public UserHasNoMoneyOrBtcException(string type)
            : base($"User doesn't have enough {type}!")
        {

        }
    }
}