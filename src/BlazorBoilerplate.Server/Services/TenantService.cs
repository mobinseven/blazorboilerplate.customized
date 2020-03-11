using AutoMapper;
using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Data.Interfaces;
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

        Tenant GetTenant();

        Task<ApiResponse> GetTenant(Guid id);

        Task<ApiResponse> PostTenant(TenantDto tenant);

        Task<ApiResponse> PutTenant(TenantDto tenant);

        Task<ApiResponse> DeleteTenant(Guid id);

        Task<ApiResponse> GetTenantUsers(Guid TenantId);

        Task<ApiResponse> AddTenantOwner(string UserName, Guid TenantId);

        Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId);

        Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserSession _userSession;

        public TenantService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IUserSession userSession)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _userSession = userSession;
        }

        #region Tenants

        public async Task<ApiResponse> GetTenants() => new ApiResponse(200, "Retrieved Tenants", await _db.Tenants.ToListAsync());

        public async Task<ApiResponse> GetTenant(Guid id) => new ApiResponse(200, "Retrieved Tenant", await _db.Tenants.FindAsync(id));

        public Tenant GetTenant()
        {
            Guid TenantId = Guid.Empty;
            //read tenantId from userSession, else use root tenant.
            if (_userSession.TenantId == Guid.Empty)
            {
                if (!_db.Tenants.Any(t => t.Title == TenantConstants.RootTenantTitle))
                {
                    _db.Tenants.Add(new Tenant { Title = TenantConstants.RootTenantTitle });
                    _db.SaveChanges();
                }
                TenantId = _db.Tenants.Where(t => t.Title == TenantConstants.RootTenantTitle).FirstOrDefault().Id;
            }
            else
            {
                TenantId = _userSession.TenantId;
            }
            return _db.Tenants.Find(TenantId);
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
            return new ApiResponse(200, "Tenant Removed", tenant);
        }

        private bool TenantExists(Guid id)
        {
            return _db.Tenants.Any(e => e.Id == id);
        }

        #endregion Tenants

        #region TenantManagement

        public async Task<ApiResponse> GetTenantUsers(Guid TenantId)
        {
            Claim userClaim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
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
                        Roles = (List<string>)(await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "Tenant User list fetched", userDtoList);
        }

        public async Task<ApiResponse> AddTenantOwner(string UserName, Guid TenantId)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(UserName);
            if (await TryAddTenantOwner(user, TenantId))
            {
                return new ApiResponse(200, "User added as tenant owner");
            }
            else
            {
                return new ApiResponse(500, "Can not add user to tenant . Maybe they are in another tenant already.");
            }
        }

        public async Task<ApiResponse> AddTenantUser(string UserName, Guid TenantId)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(UserName);
            if (await TryAddTenantClaim(user.Id, TenantId))
            {
                return new ApiResponse(200, "User added as tenant user");
            }
            else
            {
                return new ApiResponse(500, "Can not add user to tenant . Maybe they are in another tenant already.");
            }
        }

        public async Task<ApiResponse> RemoveTenantUser(Guid UserId, Guid TenantId)
        {
            if (await TryRemoveTenantClaim(UserId, TenantId))
            {
                return new ApiResponse(200, "User removed as tenant user");
            }
            else
            {
                return new ApiResponse(200, "User is not in this tenant.");
            }
        }

        #endregion TenantManagement

        private async Task<bool> TryAddTenantClaim(Guid UserId, Guid TenantId)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
            if (!userClaims.Any(c => c.Type == ClaimConstants.TenantId))//We only accept one tenant claim for each user: Single-level Multitenancy
            {
                await _userManager.AddClaimAsync(appUser, claim);
                return true;
            }
            return false;
        }

        private async Task<bool> TryRemoveTenantClaim(Guid UserId, Guid TenantId)
        {
            ApplicationUser appUser = await _userManager.FindByIdAsync(UserId.ToString());
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(appUser);
            Claim claim = new Claim(ClaimConstants.TenantId, TenantId.ToString());
            if (userClaims.Any(c => c.Type == ClaimConstants.TenantId))
            {
                await _userManager.RemoveClaimAsync(appUser, claim);
                return true;
            }
            return false;
        }

        private async Task<bool> TryAddTenantOwner(ApplicationUser User, Guid TenantId)
        {
            _userSession.TenantId = TenantId; // Set tenantId so the new manager role will only belong to the creator's tenant.
            await EnsureRoleAsync(RoleConstants.TenantManagerRoleName, "Tenant Manager",
                new string[] {
                        Permissions.Tenant.Manager,
                        Permissions.Role.Create,
                        Permissions.Role.Read,
                        Permissions.Role.Update,
                        Permissions.Role.Delete
                });
            if (await TryAddTenantClaim(User.Id, TenantId))
            {
                if (await _db.Roles.AnyAsync<ApplicationRole>(r => r.Name == RoleConstants.TenantManagerRoleName))
                {
                    IdentityResult result = await _userManager.AddToRoleAsync(User, RoleConstants.TenantManagerRoleName);
                    return result.Succeeded;
                }
                return false;
            }
            return false;
        }

        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _roleManager.FindByNameAsync(roleName)) == null)
            {
                if (claims == null)
                    claims = new string[] { };

                string[] invalidClaims = claims.Where(c => ApplicationPermissions.GetPermissionByValue(c) == null).ToArray();
                if (invalidClaims.Any())
                    throw new Exception("The following claim types are invalid: " + string.Join(", ", invalidClaims));

                ApplicationRole applicationRole = new ApplicationRole(roleName);

                var result = await _roleManager.CreateAsync(applicationRole);

                ApplicationRole role = await _roleManager.FindByNameAsync(applicationRole.Name);

                foreach (string claim in claims.Distinct())
                {
                    result = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByValue(claim)));

                    if (!result.Succeeded)
                    {
                        await _roleManager.DeleteAsync(role);
                    }
                }
            }
            else if (roleName == RoleConstants.AdminRoleName)// Ensure Admin has all permissions
            {
                ApplicationRole adminRole = await _roleManager.FindByNameAsync(roleName);
                var AllClaims = claims;
                var RoleClaims = (await _roleManager.GetClaimsAsync(adminRole)).Select(c => c.Value).ToList();
                var NewClaims = AllClaims.Except(RoleClaims);
                foreach (string claim in NewClaims)
                {
                    await _roleManager.AddClaimAsync(adminRole, new Claim(ClaimConstants.Permission, claim));
                }
                // Also we can remove deprecated permissions from all roles in db
                var DeprecatedClaims = RoleClaims.Except(AllClaims);
                var roles = await _roleManager.Roles.ToListAsync();
                foreach (string claim in DeprecatedClaims)
                {
                    foreach (var role in roles)
                    {
                        await _roleManager.RemoveClaimAsync(role, new Claim(ClaimConstants.Permission, claim));
                    }
                }
            }
        }
    }
}