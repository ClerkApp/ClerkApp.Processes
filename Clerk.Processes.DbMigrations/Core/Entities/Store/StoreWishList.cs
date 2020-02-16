using System.Collections.Generic;

namespace Clerk.Processes.DbMigrations.Core.Entities.Store
{
    public class StoreWishList
    {
        public string Name { get; set; }

        public List<string> ProductsId { get; set; }
    }
}
