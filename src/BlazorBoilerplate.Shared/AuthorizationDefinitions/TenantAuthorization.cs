using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{
    public enum TenantRole//TODO ITenant for IdentityRole? (multitenancy in roles)
    {
        Manager,
        User,
        Any
    }

    public static class TenantAuthorization
    {
        public const string TenantClaimType = "Tenant";

        public static class Policies
        {
            public const string Manager = "TenantManagerPolicy";
            public const string User = "TenantUserPoilcy";
            public const string Everyone = "TenantEveryonePolicy";
        }

        public static Claim GenerateTenantClaim(Guid TenantId, TenantRole roleInTenant) => new Claim(TenantClaimType, roleInTenant + ":" + TenantId.ToString());

        public static string GenerateTenantClaimValue(Guid TenantId, TenantRole roleInTenant) => roleInTenant + ":" + TenantId.ToString();

        public static Guid ExtractTenantId(Claim claim) => new Guid(claim.Value.Substring(claim.Value.IndexOf(':') + 1));

        public static Guid ExtractTenantId(string claimValue) => new Guid(claimValue.Substring(claimValue.IndexOf(':') + 1));

        public static TenantRole ExtractTenantRole(Claim claim) => (TenantRole)Convert.ToInt32(claim.Value.Substring(0, claim.Value.Length - claim.Value.IndexOf(':')));

        public static TenantRole ExtractTenantRole(string claimValue) => (TenantRole)Enum.Parse(typeof(TenantRole), claimValue.Substring(0, claimValue.Length - claimValue.IndexOf(':')));
    }
}