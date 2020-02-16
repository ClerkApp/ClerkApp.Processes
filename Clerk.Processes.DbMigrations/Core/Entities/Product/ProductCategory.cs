using System.Collections.Generic;
using Clerk.Processes.DbMigrations.Core.Entities.Category;
using Clerk.Processes.DbMigrations.Core.Entities.Store;

namespace Clerk.Processes.DbMigrations.Core.Entities.Product
{
    public class ProductCategory
    {
        public string Id { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; }

        public byte[] Picture { get; set; }

        public Category.Category Category { get; set; }

        public StoreCategory Store { get; set; }
        
        public List<CategorySpecification> Specifications { get; set; }
    }
}
