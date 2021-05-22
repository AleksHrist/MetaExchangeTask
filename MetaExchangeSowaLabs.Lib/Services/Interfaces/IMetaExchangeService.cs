using System.Collections.Generic;
using MetaExchangeSowaLabs.Lib.Enums;

namespace MetaExchangeSowaLabs.Lib.Services.Interfaces
{
    public interface IMetaExchangeService
    {
        Dictionary<string, List<string>> GetOptimalStepsForBestPrice(IEnumerable<string> orderBooks,
            IEnumerable<string> orderBooksUserBalance,
            TypeOfOrderEnum typeOfOrder, decimal amountOfBtc);

        void PrintOptimalStepsToStdOut(Dictionary<string, List<string>> result);
    }
}