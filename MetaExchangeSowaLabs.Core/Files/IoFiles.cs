using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace MetaExchangeSowaLabs.Core.Files
{
    public static class IoFiles
    {
        public static IEnumerable<string> ExtractNRows(string path, int numberOfRows)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();
            
            return File.ReadLines(path).Take(numberOfRows);
        }
        
    }
}