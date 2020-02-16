using System.Collections.Generic;

namespace Clerk.Processes.DbMigrations.Core.Entities.Category
{
    public class Category
    {
        public string ParentId { get; set; }

        public int Level { get; set; }

        public int Type { get; set; }

        public string TypeName { get; set; }

        public string LevelName { get; set; }

        public List<string> Meta { get; set; }
    }
}
