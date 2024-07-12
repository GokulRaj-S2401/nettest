using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DASAPI.Models
{
    public class Template
    {
        public string TempName { get; set; } = null!;
        public string TempID { get; set; } = null!;
        public string roles { get; set; } = null!;
        public string RoleID { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string Members { get; set; } = null!;
        public string child { get; set; } = null!;
        public string updatedListing { get; set; } = null!;
        public string ProofReader { get; set; } = null!;
        public string Approver { get; set; } = null!;
        public string Publisher { get; set; } = null!;

    }
}
