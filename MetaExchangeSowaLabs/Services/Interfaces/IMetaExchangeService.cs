using System.Collections.Generic;
using MetaExchangeSowaLabs.Enums;

namespace MetaExchangeSowaLabs.Services.Interfaces
{
    public interface IMetaExchangeService
    {
        Dictionary<string, List<string>> GetOptimalStepsForBestPrice(IEnumerable<string> orderBooks,
            IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc);

        void PrintOptimalStepsToStdOut(Dictionary<string, List<string>> result);
    }
}