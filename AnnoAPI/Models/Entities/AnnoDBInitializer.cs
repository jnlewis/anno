using System.Data.Entity;

namespace Anno.Models.Entities
{
    public class AnnoDBInitializer : CreateDatabaseIfNotExists<AnnoDBContext>
    {
        protected override void Seed(AnnoDBContext context)
        {
            base.Seed(context);
        }
    }
}