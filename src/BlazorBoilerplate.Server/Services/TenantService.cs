using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Services
{
    public interface ITenantService
    {
        Task<ApiResponse> GetTenants();

        Task<ApiResponse> GetTenant(Guid id);

        Task<ApiResponse> PostTenant(TenantDto tenant);

        Task<ApiResponse> PutTenant(TenantDto tenant);

        Task<ApiResponse> DeleteTenant(Guid id);

        Task<ApiResponse> GetTenantUsers(Guid TenantId);

        Task<ApiResponse> AddTenantManager(string UserName, Guid TenantId);

        Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId);

        Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _autoMapper;

        public TenantService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IMapper autoMapper)
        {
            _db = db;
            _userManager = userManager;
            _autoMapper = autoMapper;
        }

        #region Tenants

        public async Task<ApiResponse> GetTenants() => new ApiResponse(200, "Retrieved Tenants", await _db.Tenants.ToListAsync());

        public async Task<ApiResponse> GetTenant(Guid id) => new ApiResponse(200, "Retrieved Tenant", await _db.Tenants.FindAsync(id));

        public async Task<ApiResponse> GetUserTenant(ClaimsPrincipal User)
        {
            Claim claim = User.Claims.FirstOrDefault(c => c.Type == TenantClaims.Tenant);
            Tenant tenant = null;
            if (claim != null)
            {
                Guid TenantId = TenantClaims.ExtractTenantId(claim.Value);
                tenant = await _db.Tenants.FindAsync(TenantId);
            }
            return new ApiResponse(200, "Get User Tenant Successful", tenant);
        }

        public async Task<ApiResponse> PutTenant(TenantDto tenant)
        {
            Tenant t = _db.Tenants.Find(tenant.Id);
            t.Title = tenant.Title;
            try
            {
                await _db.SaveChangesAsync();
                return new ApiResponse(200, "Tenant Updated. ", t);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(tenant.Id))
                {
                    return new ApiResponse(404, "Tenant Not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<ApiResponse> PostTenant(TenantDto tenant)
        {
            Tenant t = new Tenant
            {
                Title = tenant.Title
            };
            await _db.Tenants.AddAsync(t);
            await _db.SaveChangesAsync();

            return new ApiResponse(200, "Tenant Created.", t);
        }

        public async Task<ApiResponse> DeleteTenant(Guid id)
        {
            Tenant tenant = await _db.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return new ApiResponse(404, "Tenant Not found");
            }

            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
            return new ApiResponse(200, "Tenant Removed", _autoMapper.Map(tenant, new TenantDto()));
        }

        private bool TenantExists(Guid id)
        {
            return _db.Tenants.Any(e => e.Id == id);
        }

        #endregion Tenants

        #region TenantManagement

        public async Task<ApiResponse> GetTenantUsers(Guid TenantId)
        {
            Claim userClaim = TenantClaims.GenerateTenantClaim(TenantId, TenantRole.User);
            List<UserInfoDto> userDtoList = new List<UserInfoDto>();
            IList<ApplicationUser> listResponse;
            try
            {
                listResponse = await _userManager.GetUsersForClaimAsync(userClaim);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            // create the dto object with mapped properties and fetch roles associated with each user
            try
            {
                foreach (ApplicationUser applicationUser in listResponse)
                {
                    userDtoList.Add(new UserInfoDto
                    {
                        FirstName = applicationUser.FirstName,
                        LastName = applicationUser.LastName,
                        UserName = applicationUser.UserName,
                        Email = applicationUser.Email,
                        UserId = applicationUser.Id,
                        //Roles = (List<string>)(await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true)) // TODO multitenancy in roles?
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "Tenant User list fetched", userDtoList);
        }

        public async Task<ApiResponse> AddTenantManager(string UserName, Guid TenantId)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (await TryAddTenantClaim(user.Id, TenantId, TenantRole.Manager))
                return new ApiResponse(200, "User added as tenant Manager");
            else
                return new ApiResponse(500, "Can not add user to tenant . Maybe they are in another tenant already.");
        }

        public async Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (await TryAddTenantClaim(user.Id, TenantId, TenantRole.User))
                return new ApiResponse(200, "User added as tenant user");
            else
                return new ApiResponse(500, "Can not add user to tenant . Maybe they are in another tenant already.");
        }

        public async Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId)
        {
            if (await TryRemoveTenantClaim(UserId, TenantId, TenantRole.User))
                return new ApiResponse(200, "User removed as tenant user");
            else
                return new ApiResponse(200, "User is not in this tenant.");
        }

        #endregion TenantManagement

        private async Task<bool> TryAddTenantClaim(Guid UserId, Guid TenantId, TenantRole claimType)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = TenantClaims.GenerateTenantClaim(TenantId, claimType);
            if (!userClaims.Any(c => c.Type == TenantClaims.Tenant))//We only accept tenant claim for each user: Single-level Multitenancy
            {
                await _userManager.AddClaimAsync(appUser, claim);
                return true;
            }
            return false;
        }

        private async Task<bool> TryRemoveTenantClaim(Guid UserId, Guid TenantId, TenantRole claimType)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = TenantClaims.GenerateTenantClaim(TenantId, claimType);
            if (userClaims.Contains(claim))
            {
                await _userManager.RemoveClaimAsync(appUser, claim);
                return true;
            }
            return false;
        }
    }
}