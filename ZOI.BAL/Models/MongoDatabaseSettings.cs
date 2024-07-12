namespace DASAPI.Models
{
    public class MongoDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string TemplateCollectionName { get; set; } = null!;
    }
}
