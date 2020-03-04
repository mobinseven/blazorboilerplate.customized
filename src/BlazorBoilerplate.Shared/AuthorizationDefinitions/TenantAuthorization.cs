using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{//TODO https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/ : Role assignments should be managed by the customer, not by the SaaS provider.
    public static class TenantDefinitions
    {
        public const string PublicTenantTitle = "Public";

        public const string ClaimType = "TenantId";

        public const string Owner = "TenantOwner";

        public const string Policy = "TenantPolicy";

        public static AuthorizationPolicy TenantPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimType)
                .Build();
        }
    }
}