using Microsoft.AspNetCore.Mvc;

namespace postplateapi.ResponseHelper
{
    public class ActionResultResponseHelper
    {
        public IActionResult CreateResponse(bool success, int status, string message, object? data = null)
        {
            return new ObjectResult(new
            {
                success,
                status,
                message,
                data,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = status
            };
        }
    }
}
