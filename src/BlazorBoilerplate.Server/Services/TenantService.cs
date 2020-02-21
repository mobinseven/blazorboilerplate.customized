using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Tenant;
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

        Task<ApiResponse> PutTenant(Guid id, TenantDto tenant);

        Task<ApiResponse> PostTenant(TenantDto tenant);

        Task<ApiResponse> DeleteTenant(Guid id);

        Task<ApiResponse> GetTenantUsers(ClaimsPrincipal User);

        Task<ApiResponse> AddTenantManager(UserInfoDto userInfoDto, TenantDto tenant);

        Task<ApiResponse> RemoveTenantManager(UserInfoDto userInfoDto, TenantDto tenant);

        Task<ApiResponse> AddTenantUser(UserInfoDto userInfoDto, TenantDto tenant);

        Task<ApiResponse> RemoveTenantUser(UserInfoDto userInfoDto, TenantDto tenant);
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _autoMapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public TenantService(ApplicationDbContext db, IMapper autoMapper, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _autoMapper = autoMapper;
            _userManager = userManager;
        }

        #region Tenants

        public async Task<ApiResponse> GetTenants()
        {
            return new ApiResponse(200, "Retrieved Tenants", _autoMapper.Map<List<TenantDto>>(await _db.Tenants.ToListAsync()));
        }

        public async Task<ApiResponse> GetTenant(Guid id)
        {
            return new ApiResponse(200, "Retrieved Tenant", _autoMapper.Map<TenantDto>(await _db.Tenants.FindAsync(id)));
        }

        public async Task<ApiResponse> PutTenant(Guid id, TenantDto tenant)
        {
            _db.Tenants.Find(id);
            Tenant t = _db.Tenants.Find(id);
            _autoMapper.Map(tenant, t);
            try
            {
                await _db.SaveChangesAsync();
                return new ApiResponse(200, "Tenant Updated", _autoMapper.Map(t, new TenantDto()));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
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
            Tenant t = new Tenant();
            await _db.Tenants.AddAsync(_autoMapper.Map(tenant, t));
            await _db.SaveChangesAsync();

            return new ApiResponse(200, "Tenant Created", _autoMapper.Map(t, new TenantDto()));
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

        #region TenantUsers

        public async Task<ApiResponse> AddTenantManager(UserInfoDto userInfoDto, TenantDto tenant)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(userInfoDto.UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            if (userClaims.Contains(new Claim(Claims.TenantId, tenant.Id.ToString())))
            {
                await _userManager.AddClaimAsync(appUser, new Claim(Policies.IsTenantManager, "true"));
                return new ApiResponse(200, "User added as tenant manager", userInfoDto);
            }
            return new ApiResponse(200, "User is not in tenant", userInfoDto);
        }

        public async Task<ApiResponse> RemoveTenantManager(UserInfoDto userInfoDto, TenantDto tenant)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(userInfoDto.UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            if (userClaims.Contains(new Claim(Claims.TenantId, tenant.Id.ToString())))
            {
                await _userManager.RemoveClaimAsync(appUser, new Claim(Policies.IsTenantManager, "true"));
                return new ApiResponse(200, "User added as tenant manager", userInfoDto);
            }
            return new ApiResponse(200, "User is not in tenant", userInfoDto);
        }

        public async Task<ApiResponse> AddTenantUser(UserInfoDto userInfoDto, TenantDto tenant)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(userInfoDto.UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            if (!userClaims.Contains(new Claim(Claims.TenantId, tenant.Id.ToString())))
            {
                await _userManager.AddClaimAsync(appUser, new Claim(Claims.TenantId, tenant.Id.ToString()));
                return new ApiResponse(200, "User added as tenant user", userInfoDto);
            }
            return new ApiResponse(200, "User is already in tenant", userInfoDto);
        }

        public async Task<ApiResponse> RemoveTenantUser(UserInfoDto userInfoDto, TenantDto tenant)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(userInfoDto.UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            if (userClaims.Contains(new Claim(Claims.TenantId, tenant.Id.ToString())))
            {
                await _userManager.RemoveClaimAsync(appUser, new Claim(Claims.TenantId, tenant.Id.ToString()));
                return new ApiResponse(200, "User removed as tenant user", userInfoDto);
            }
            return new ApiResponse(200, "User is not in tenant", userInfoDto);
        }

        public async Task<ApiResponse> GetTenantUsers(ClaimsPrincipal User)
        {
            ApplicationUser tenantOwner = await _userManager.GetUserAsync(User);
            IList<Claim> tenantOwnerClaims = await _userManager.GetClaimsAsync(tenantOwner);
            Claim tenantIdClaim = tenantOwnerClaims.FirstOrDefault(c => c.Type == "tenantid");
            Guid TenantId = Guid.Empty;
            if (tenantIdClaim != null)
            {
                TenantId = Guid.Parse(tenantIdClaim.Value);
                List<UserInfoDto> userDtoList = new List<UserInfoDto>();
                IList<ApplicationUser> listResponse;

                try
                {
                    listResponse = await _userManager.GetUsersForClaimAsync(tenantIdClaim);
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
            return new ApiResponse(200, "No tenant defined");
        }

        #endregion TenantUsers
    }
}