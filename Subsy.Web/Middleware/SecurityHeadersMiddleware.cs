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

        // Content Security Policy
        // 'unsafe-inline' is required because several views use inline <script> blocks
        // and inline style attributes. Tighten to nonce-based CSP when views are refactored.
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
            "img-src 'self' data: https://www.google.com https://t0.gstatic.com https://t1.gstatic.com https://t2.gstatic.com https://t3.gstatic.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self'; " +
            "base-uri 'self';";

        await _next(context);
    }
}