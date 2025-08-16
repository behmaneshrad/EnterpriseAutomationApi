namespace EnterpriseAutomation.Api.Middelware.AuthorizeMIddelware
{
    public class AuthorizeMessageMW
    {
        private readonly RequestDelegate _request;
        public AuthorizeMessageMW(RequestDelegate request)
        {
            _request = request;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                if (context.Response.StatusCode == 401)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"دسترسی غیرمجاز - لطفاً وارد شوید.\"}");
                }
                else if (context.Response.StatusCode == 403)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"شما مجوز دسترسی به این بخش را ندارید.\"}");
                }
            }
           await _request(context);
        }
    }
}
