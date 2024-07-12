using static ZOI.BAL.Utilities.Constants;

namespace DASAPI.Models
{
    public class JsonResponse
    {
        public int ID { get; set; }
        public string Status { get; set; } = APIResponseStatus.Success;
        public string Message { get; set; } = APIResponseMessage.SuccessMessage;
        public dynamic Data { get; set; }
    }
}
