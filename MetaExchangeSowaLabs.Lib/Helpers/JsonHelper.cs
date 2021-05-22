using System.Collections.Generic;
using Newtonsoft.Json;

namespace MetaExchangeSowaLabs.Lib.Helpers
{
    public static class JsonHelper
    {
        public static List<T> DeserializeEntity<T>(IEnumerable<string> jsonRows)
        {
            var deserializeEntities = new List<T>();
            foreach (var x in jsonRows)
            {
                //Basic check if the json is properly formatted
                var indexOfTab = x.IndexOf('\t');
                var indexOfCurlyBracer = x.IndexOf('{');
                if (indexOfTab == -1 || indexOfCurlyBracer == -1)
                    throw new JsonException("Json not properly formatted!");


                //Get the timestamp as ID so we can connect the balance and the data
                var orderBookId = x.Remove(indexOfTab);

                //remove the timestamp so the row can be deserialized
                var cleanJson = x.Remove(0, indexOfCurlyBracer);

                //Deserialize to .Net classes and save the ID
                try
                {
                    dynamic deserializedEntity = JsonConvert.DeserializeObject<T>(cleanJson);
                    if (deserializedEntity != null) deserializedEntity.OrderBookId = orderBookId;

                    deserializeEntities.Add((T) deserializedEntity);
                }
                catch (JsonException e)
                {
                    throw new JsonException("Couldn't serialize the document! : " + e);
                }
            }

            return deserializeEntities;
        }
    }
}