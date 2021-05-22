using System;
using System.Collections.Generic;

namespace MetaExchangeSowaLabs.Services.Helpers
{
    public static class PrintToStdHelper
    {
        public static void PrintOptimalStepsToStdOut(Dictionary<string, List<string>> result)
        {
            foreach (var (key, value) in result)
            {
                Console.WriteLine($"For cryptoexchange with id {key} do this: ");
                value.ForEach(Console.WriteLine);
            }
        }
    }
}