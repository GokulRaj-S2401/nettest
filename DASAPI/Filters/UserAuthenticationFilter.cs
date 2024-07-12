using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace DASAPI.Filters
{
    public class UserAuthenticationFilter: ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string Token = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(Token))
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Unauthorized);
            }
            else if (Token != GetSection("Token"))
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Unauthorized);
            }
        }

        public static string GetSection(string keys)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            string json = File.ReadAllText(file);
            JObject jsonObject = JObject.Parse(json);
            string jsonValue = jsonObject != null && jsonObject.Count > 0 ? Convert.ToString(jsonObject[keys]) : string.Empty;
            return jsonValue;
        }
    }
}
