using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Model;

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
                currentUser.UserId = int.TryParse(context.User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
                currentUser.CurrentGymId = int.TryParse(context.User.FindFirst("CurrentGymId")?.Value, out var gid) ? gid : null;
                currentUser.GymRole = context.User.FindFirst("GymRole")?.Value;
                currentUser.PlatformRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? context.User.FindFirst("Role")?.Value;
                currentUser.IsAuthenticated = true;
            }

            await _next(context);
        }
    }
}