using System.Collections.Generic;
using Clerk.Processes.DbMigrations.Core.Entities.Store;

namespace Clerk.Processes.DbMigrations.Core.Entities.Product
{
    public class Product
    {
        public string Id { get; set; }

        public string SupplierId { get; set; }

        public string CategoryId { get; set; }

        public List<string> ProductName { get; set; }

        public StoreProduct Store { get; set; }
    }
}
