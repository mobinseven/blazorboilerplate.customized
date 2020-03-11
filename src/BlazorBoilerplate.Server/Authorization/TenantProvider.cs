using BlazorBoilerplate.Server.Data.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Authorization
{
    public sealed class TenantProvider :
    ITenantProvider
    {
        private readonly IHttpContextAccessor _accessor;

        public TenantProvider(
            IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid GetId()
        {
            Guid TenantId = Guid.Empty;
            if (_accessor.HttpContext != null)
            {
                System.Security.Claims.Claim tenantClaim = _accessor.HttpContext.User.Claims.FirstOrDefault(predicate: c => c.Type == ClaimConstants.TenantId);
                if (tenantClaim != null) // user belongs to a tenant
                {
                    TenantId = Guid.Parse(tenantClaim.Value);
                }
            }
            return TenantId;
        }
    }

    public interface ITenantProvider
    {
        Guid GetId();
    }
}