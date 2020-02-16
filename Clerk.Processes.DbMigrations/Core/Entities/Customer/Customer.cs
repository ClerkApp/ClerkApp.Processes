using System.Collections.Generic;
using Clerk.Processes.DbMigrations.Core.Entities.Store;

namespace Clerk.Processes.DbMigrations.Core.Entities.Customer
{
    public class Customer
    {
        public string Id { get; set; }

        public IdentityCard IdentityCard { get; set; }

        public BillingAddress BillingAddress { get; set; }

        public List<CreditCard> CreditCards { get; set; }

        public List<string> Phone { get; set; }

        public List<string> Emails { get; set; }

        public StoreCustomer Store { get; set; }
    }
}
