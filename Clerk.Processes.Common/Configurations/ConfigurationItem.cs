using System.Collections.Generic;

namespace Clerk.Processes.Common.Configurations
{
    public class ConfigurationItem
    {
        public string Endpoint { get; set; }

        public List<DbToPopulate> ToPopulate { get; set; }

        public List<ItemToDelete> ToDelete { get; set; }

        public class ItemToDelete
        {
            public string Database { get; set; }

            public string Collection { get; set; }
        }

        public class DbToPopulate
        {
            public string Database { get; set; }
        }
    }
}
