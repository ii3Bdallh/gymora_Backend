using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Enum;
using Domain.Model.Auth;

namespace Api.Middlewares
{
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            CurrentUser currentUser)
        {
            var user = context.User;



            if (user?.Identity?.IsAuthenticated == true)
            {
                currentUser.UserId = int.Parse(user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");

                currentUser.IsAdmin = user.IsInRole(nameof(RoleType.Admin)) || user.IsInRole(nameof(RoleType.Owner));
  
            }

            await _next(context);
        }
    }

}
