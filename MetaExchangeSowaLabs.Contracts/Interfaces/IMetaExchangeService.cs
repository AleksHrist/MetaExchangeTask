using System.Collections.Generic;
using MetaExchangeSowaLabs.Core.Enums;

namespace MetaExchangeSowaLabs.Contracts.Interfaces
{
    public interface IMetaExchangeService
    {
        Dictionary<string, List<string>> GetOptimalStepsForBestPrice(IEnumerable<string> orderBooks,
            IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc);
    }
}