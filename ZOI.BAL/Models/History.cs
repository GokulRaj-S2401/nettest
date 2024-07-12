namespace DASAPI.Models
{
    public class History :IDisposable
    {
        public string? objectID { get; set; }
        public string? rowID { get; set; }
        public string? blockID { get; set; }
        public string? data { get; set; }
        public string? createdBy { get; set; }
        public string? createdByID { get; set; }
        public string? createdOn { get; set; }

        public void Dispose()
        {
        }
    }
}
