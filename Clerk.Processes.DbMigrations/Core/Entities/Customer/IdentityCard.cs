using System;
using System.Collections.Generic;

namespace Clerk.Processes.DbMigrations.Core.Entities.Customer
{
    public class IdentityCard
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        public List<Address> Addresses { get; set; }
    }
}
