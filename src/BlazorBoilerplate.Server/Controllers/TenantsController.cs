using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ApiResponse _invalidModel;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TenantsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ITenantService tenantService)
        {
            _db = db;
            _userManager = userManager;
            _tenantService = tenantService;
            _invalidModel = new ApiResponse(400, "Tenant Model is Invalid");
        }

        // GET: api/Tenants
        [HttpGet]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> GetTenants() => await _tenantService.GetTenants();

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetTenant(Guid id) => await _tenantService.GetTenant(id);

        [HttpGet("GetUserTenant")]
        [Authorize]
        public async Task<ApiResponse> GetUserTenant()
        {
            Claim claim = User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.TenantId);
            Guid TenantId = Guid.Empty;
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (claim != null)
            {
                TenantId = Guid.Parse(claim.Value);
            }
            return await _tenantService.GetTenant(TenantId);
        }

        // PUT: api/Tenants/5
        [HttpPut]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> PutTenant([FromBody] TenantDto tenant) => ModelState.IsValid ? await _tenantService.PutTenant(tenant) : _invalidModel;

        // POST: api/Tenants
        [HttpPost]
        [Authorize]
        public async Task<ApiResponse> PostTenant([FromBody] TenantDto tenant)
        {
            if (ModelState.IsValid)
            {
                ApiResponse apiResponse = await _tenantService.PostTenant(tenant);
                if (apiResponse.StatusCode == 200)
                {
                    await _tenantService.AddTenantOwner(User.Identity.Name, ((Tenant)apiResponse.Result).Id);
                }

                return apiResponse;
            }
            else
            {
                return _invalidModel;
            }
        }

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> DeleteTenant(Guid id) => await _tenantService.DeleteTenant(id);

        [HttpGet("Users/{tenantId}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> GetTenantUsers(Guid tenantId) => await _tenantService.GetTenantUsers(tenantId);

        [HttpDelete("Users/{tenantId}/{userId}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> RemoveTenantUser(Guid tenantId, Guid userId) => await _tenantService.RemoveTenantUser(userId, tenantId);

        [HttpPost("Users/{tenantId}/{userName}")]
        [Authorize(Permissions.Tenant.Manager)]
        public async Task<ApiResponse> AddTenantUser(Guid tenantId, string userName) => await _tenantService.AddTenantUser(userName, tenantId);
    }
}