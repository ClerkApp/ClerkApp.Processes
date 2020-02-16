namespace Clerk.Processes.DbMigrations.Core.Entities.Store
{
    public class StoreCategory
    {
        public bool ShowUpFront { get; set; }

        public bool EnableDeposit { get; set; }

        public byte[] Thumbnail { get; set; }

        public string Description { get; set; }
    }
}
