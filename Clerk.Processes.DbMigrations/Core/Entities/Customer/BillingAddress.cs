namespace Clerk.Processes.DbMigrations.Core.Entities.Customer
{
    public class BillingAddress
    {
        public Address Address { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
