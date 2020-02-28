using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Authorization
{
    public class TenantRequirement : IAuthorizationRequirement
    {
        public TenantRole TenantRole { get; private set; }

        public TenantRequirement(TenantRole tenantRole)
        {
            TenantRole = tenantRole;
        }

        public TenantRequirement()
        {
            TenantRole = TenantRole.Any;
        }
    }

    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        //private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpContext _httpContext;

        public TenantAuthorizationHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            //_userManager = userManager;
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       TenantRequirement requirement)
        {
            //ApplicationUser user = await _userManager.GetUserAsync(context.User);//ConcurrencyException
            //IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);

            IList<Claim> userClaims = _httpContext.User.Claims.ToList();
            object tenantId = _httpContext.Request.RouteValues["TenantId"];

            if (tenantId != null)// tenantId specified
            {
                if (Guid.TryParse(tenantId.ToString(), out Guid TenantId))
                {
                    if (requirement.TenantRole == TenantRole.Any)// tenant role not specified
                    {
                        if (userClaims.Any(c => c.Type == TenantAuthorization.TenantClaimType && TenantAuthorization.ExtractTenantId(c) == TenantId))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else// tenant role specified
                    {
                        if (userClaims.Any(c => c.Type == TenantAuthorization.TenantClaimType &&
                        c.Value == TenantAuthorization.GenerateTenantClaimValue(TenantId, requirement.TenantRole)))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }
            else// tenantId not specified
            {
                if (requirement.TenantRole == TenantRole.Any)// tenant role not specified
                {
                    if (userClaims.Any(c => c.Type == TenantAuthorization.TenantClaimType))
                    {
                        context.Succeed(requirement);
                    }
                }
                else// tenant role specified
                {
                    if (userClaims.Any(c => c.Type == TenantAuthorization.TenantClaimType && TenantAuthorization.ExtractTenantRole(c) == requirement.TenantRole))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}