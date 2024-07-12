namespace DASAPI.Models
{
    public class Roles
    {
        public string? roleID { get; set; }
        public string? roleName { get; set; }
        public object? members { get; set; }
        public string? documentID { get; set; }
        public string? memberID { get; set; }
        public bool? isAdd { get; set; }
        public object? rights { get; set; }
        public string? objectID { get; set; }
        public bool isCollaborator { get; set; } = false;
    }
}
