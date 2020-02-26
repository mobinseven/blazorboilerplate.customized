using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BlazorBoilerplate.Shared.AuthorizationDefinitions
{
    public enum TenantRole//TODO ITenant for IdentityRole? (multitenancy in roles)
    {
        Manager,
        User
    }

    //public class TenantClaim
    //{
    //    public TenantRole RoleInTenant { get; set; }
    //    public Guid TenantId { get; set; }
    //    public Claim Claim { get; set; }

    //    public TenantClaim(Claim claim)
    //    {
    //        Claim = claim;
    //        TenantId = TenantClaims.ExtractTenantId(claim);
    //        RoleInTenant = TenantClaims.ExtractTenantRole(claim);
    //    }

    //    public TenantClaim(string claimValue)
    //    {
    //        TenantId = TenantClaims.ExtractTenantId(claimValue);
    //        RoleInTenant = TenantClaims.ExtractTenantRole(claimValue);
    //    }
    //}

    public static class TenantClaims
    {
        public const string Tenant = "Tenant";

        public static Claim GenerateTenantClaim(Guid TenantId, TenantRole roleInTenant) => new Claim(Tenant, roleInTenant + ":" + TenantId.ToString());

        public static Guid ExtractTenantId(Claim claim) => new Guid(claim.Value.Substring(claim.Value.IndexOf(':') + 1));

        public static Guid ExtractTenantId(string claimValue) => new Guid(claimValue.Substring(claimValue.IndexOf(':') + 1));

        public static TenantRole ExtractTenantRole(Claim claim) => (TenantRole)Convert.ToInt32(claim.Value.Substring(0, claim.Value.Length - claim.Value.IndexOf(':')));

        public static TenantRole ExtractTenantRole(string claimValue) => (TenantRole)Enum.Parse(typeof(TenantRole), claimValue.Substring(0, claimValue.Length - claimValue.IndexOf(':')));
    }
}