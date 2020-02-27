using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Authorization
{
    public class TenantRequirement : IAuthorizationRequirement
    {
    }

    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpContext _httpContext;

        public TenantAuthorizationHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       TenantRequirement requirement)
        {
            var tenantId = _httpContext.Request.RouteValues["TenantId"];
            if (Guid.TryParse(tenantId.ToString(), out Guid TenantId))
            {
                var users = await _userManager.GetUsersForClaimAsync(TenantClaims.GenerateTenantClaim(TenantId, TenantRole.Manager));
                var user = users.FirstOrDefault(u => u.UserName == context.User.Identity.Name);
                if (user != null)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}