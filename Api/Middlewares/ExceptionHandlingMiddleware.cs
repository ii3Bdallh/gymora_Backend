using Application.DTO;
using Application.DTO.Exceptions;

namespace Api.Middalewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("BAD_REQUEST", ex.Message));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("UNAUTHORIZED", ex.Message));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("NOT_FOUND", ex.Message));
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("FORBIDDEN", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.ContentType = "application/json";
                //await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("INTERNAL_ERROR", "An unexpected error occurred. Please try again later."));
                await httpContext.Response.WriteAsJsonAsync(Result<object>.Failure("INTERNAL_ERROR", ex.Message));
            }
        }
    }
}
