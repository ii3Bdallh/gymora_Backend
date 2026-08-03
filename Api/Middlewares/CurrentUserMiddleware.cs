using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Model;
using Infrastructure.Constant;

namespace Api.Middlewares
{
    // Api/Middlewares/CurrentUserMiddleware.cs
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, CurrentUser currentUser)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                currentUser.UserId = int.TryParse(context.User.FindFirst(JwtClaimsNames.UserId)?.Value, out var uid) ? uid : 0;
                currentUser.CurrentPersonId = int.TryParse(context.User.FindFirst(JwtClaimsNames.CurrentPersonId)?.Value, out var sid) ? sid : null;
                currentUser.CurrentGymId = int.TryParse(context.User.FindFirst(JwtClaimsNames.CurrentGymId)?.Value, out var gid) ? gid : null;
                currentUser.GymRole = context.User.FindFirst(JwtClaimsNames.GymRole)?.Value;
                currentUser.PlatformRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? context.User.FindFirst("Role")?.Value;
                currentUser.IsAuthenticated = true;
            }

            await _next(context);
        }
    }
}