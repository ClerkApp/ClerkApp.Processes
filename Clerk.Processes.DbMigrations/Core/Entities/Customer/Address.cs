namespace Clerk.Processes.DbMigrations.Core.Entities.Customer
{
    public class Address
    {
        public string State { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string CityLocation { get; set; }

        public int Zip { get; set; }
    }
}
