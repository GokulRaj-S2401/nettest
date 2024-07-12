namespace DASAPI.Models
{
    public class DocumentCollection
    {
        public string? ObjectID { get; set; }
        public string? CollectionName { get; set; }
        public string? RowID { get; set; }
        public string? BlockID { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public dynamic? Data { get; set; }
        public int? IndexOfRow { get; set; } = 0;
        public string? UserID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByID { get; set; }
        public bool? KeyValue { get; set; }
        public bool? IsAdd { get; set; }
    }
}
