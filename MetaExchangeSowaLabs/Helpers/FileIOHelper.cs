using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetaExchangeSowaLabs.Helpers
{
    static class FileIOHelper
    {
        public static IEnumerable<string> ExtractNRows(string path, int numberOfRows)
        {
             return File.ReadLines(path).Take(numberOfRows);
        }
    }
}