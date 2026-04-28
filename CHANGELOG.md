# Changelog

All notable changes to this project will be documented in this file.

## [0.12.0] - 2026-04-28

### Security
- Add allowlist-based Content-Security-Policy header to `SecurityHeadersMiddleware`
  - Whitelisted CDN origins: `cdn.jsdelivr.net`, `cdn.tailwindcss.com`
  - Directives: `default-src 'self'`, `script-src`, `style-src`, `img-src`, `font-src`, `connect-src`, `frame-ancestors 'none'`, `form-action 'self'`, `base-uri 'self'`

## [0.11.0]

### Security
- Enable HTTPS redirection; restrict Swagger UI to development environment

## [0.10.0]

### Security
- Fail fast when JWT signing key is missing (remove hardcoded fallback)

## [0.9.0]

### Security
- Restrict Hangfire dashboard to Admin role

## [0.8.0] and earlier

- See git history
