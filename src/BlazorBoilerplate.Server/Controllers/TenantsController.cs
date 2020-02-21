using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Authorization;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.Dto.Tenant;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]//TODO Roles of managing tenants
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ApiResponse _invalidModel;

        public TenantsController(ITenantService tenantService)
        {
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

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> PutTenant(Guid id, [FromBody] TenantDto tenant) => ModelState.IsValid ? await _tenantService.PutTenant(id, tenant) : _invalidModel;

        // POST: api/Tenants
        [HttpPost]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> PostTenant([FromBody] TenantDto tenant) => ModelState.IsValid ? await _tenantService.PostTenant(tenant) : _invalidModel;

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> DeleteTenant(Guid id) => await _tenantService.DeleteTenant(id);

        [HttpPut("AddTenantManager")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> AddTenantManager([FromBody]  UserInfoDto userInfoDto, [FromBody] TenantDto tenantDto) => ModelState.IsValid ? await _tenantService.AddTenantManager(userInfoDto, tenantDto) : _invalidModel;

        [HttpPut("RemoveTenantManager")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> RemoveTenantManager([FromBody]  UserInfoDto userInfoDto, [FromBody] TenantDto tenantDto) => ModelState.IsValid ? await _tenantService.RemoveTenantManager(userInfoDto, tenantDto) : _invalidModel;

        [HttpGet("Users")]
        [Authorize(Policy = Policies.IsTenantManager)]
        public async Task<ApiResponse> GetTenantUsers() => await _tenantService.GetTenantUsers(User);

        [HttpPut("RemoveTenantUser")]
        [Authorize(Policy = Policies.IsTenantManager)]
        public async Task<ApiResponse> RemoveTenantUser([FromBody]  UserInfoDto userInfoDto, [FromBody] TenantDto tenantDto) => ModelState.IsValid ? await _tenantService.RemoveTenantUser(userInfoDto, tenantDto) : _invalidModel;

        [HttpPut("AddTenantUser")]
        [Authorize(Policy = Policies.IsTenantManager)]
        public async Task<ApiResponse> AddTenantUser([FromBody]  UserInfoDto userInfoDto, [FromBody] TenantDto tenantDto) => ModelState.IsValid ? await _tenantService.AddTenantUser(userInfoDto, tenantDto) : _invalidModel;
    }
}