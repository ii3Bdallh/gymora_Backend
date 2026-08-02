using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Model;
using Domain.Enum;

namespace Api.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class GymAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public GymAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var currentUser = context.HttpContext.RequestServices.GetRequiredService<CurrentUser>();

            if (!currentUser.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // SuperAdmin bypasses all gym checks
            if (currentUser.IsSuperAdmin)
            {
                return;
            }

            // Check if user is associated with any gym
            if (!currentUser.CurrentGymId.HasValue)
            {
                context.Result = new ForbidResult();
                return;
            }

            // If no roles specified, just check gym access
            if (_allowedRoles == null || _allowedRoles.Length == 0)
            {
                return;
            }

            // Check if current user's gym role is allowed
            var userGymRole = currentUser.GymRole;

            if (userGymRole == GymRoleString.Owner)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(userGymRole) || !_allowedRoles.Contains(userGymRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
