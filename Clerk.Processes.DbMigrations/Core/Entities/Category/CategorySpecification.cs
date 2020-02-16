namespace Clerk.Processes.DbMigrations.Core.Entities.Category
{
    public class CategorySpecification
    {
        public string Name { get; set; }

        public bool ImportantType { get; set; }

        public int ImportantLevel { get; set; }
    }
}
