using System.Collections.Generic;

namespace Clerk.Processes.DbMigrations.Core.Entities.Product
{
    public class ProductPicture
    {
        public byte[] Front { get; set; }

        public byte[] Back { get; set; }

        public List<byte[]> Others { get; set; }
    }
}
