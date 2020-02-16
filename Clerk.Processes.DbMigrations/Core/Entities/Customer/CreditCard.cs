using System;

namespace Clerk.Processes.DbMigrations.Core.Entities.Customer
{
    public class CreditCard
    {
        public string HolderName { get; set; }

        public DateTime ExpireDate { get; set; }

        public string Seria { get; set; }
    }
}
