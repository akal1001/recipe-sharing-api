namespace postplateapi
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        //private const string API_KEY_HEADER = "X-API-KEY";
        //private const string VALID_API_KEY = "my-secret-api-key";
        private const string API_KEY_HEADER = "x-app-tag";
        private const string VALID_API_KEY = "wrx6tU8KyQpUTh5p4fXrFF1aPhOctU87KyQpUTh5p4fXrFF1aPhOctU8";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var apiKey = context.Request.Headers[API_KEY_HEADER].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey) || apiKey != VALID_API_KEY)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized access.");
                return;
            }
            await _next(context);
        }
    }
}
