using System.Collections.Generic;
using System.IO;
using Clerk.Processes.Common.Configurations;
using Newtonsoft.Json;

namespace Clerk.Processes.Common
{
    public static class Configuration
    {
        public static IEnumerable<ConfigurationItem> Init()
        {
            return Read<ConfigurationItem>("configuration.json");
        }

        public static IEnumerable<T> Read<T>(string path)
        {
            var rows = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));
            return rows;
        }
    }
}
