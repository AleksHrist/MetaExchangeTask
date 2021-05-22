using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetaExchangeSowaLabs.Lib.Helpers
{
    public static class FileIOHelper
    {
        public static IEnumerable<string> ExtractNRows(string path, int numberOfRows)
        {
             return File.ReadLines(path).Take(numberOfRows);
        }
    }
}