namespace Subsy.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // clickjacking koruması
        headers["X-Frame-Options"] = "DENY";

        // MIME confusion attack koruması
        headers["X-Content-Type-Options"] = "nosniff";

        // Referrer bilgisini sınırla 
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // HTTPS zorunlu — tarayıcı 1 yıl boyunca sadece HTTPS kulansın
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // XSS koruması 
        headers["X-XSS-Protection"] = "1; mode=block";

        // Permissions Policy
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        await _next(context);
    }
}