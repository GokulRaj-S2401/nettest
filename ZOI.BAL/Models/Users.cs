namespace DASAPI.Models
{
    public class Users
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Mobile { get; set; }
        public object? Role { get; set; }
        public object? ReportingTo { get; set; }
        public object? Department { get; set; }
        public string? ObjectID { get; set; }
        public string? CreatedBy { get; set; }
        public string? Url { get; set; }
        public bool? IsInvite { get; set; } = false;
        public string? RequestToken { get; set; }

    }
}
