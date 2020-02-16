using System;
using System.Collections.Generic;
using Clerk.Processes.DbMigrations.Core.Enums;

namespace Clerk.Processes.DbMigrations.Core.Entities.Store
{
    public class StoreCustomer
    {
        public List<AccountGroup> Groups { get; set; }

        public bool StatusLock { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastLogDate { get; set; }

        public List<StoreWishList> WishLists { get; set; }
    }
}
