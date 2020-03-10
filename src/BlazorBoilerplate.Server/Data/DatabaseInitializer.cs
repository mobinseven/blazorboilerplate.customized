﻿using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Data
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly PersistedGrantDbContext _persistedGrantContext;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger _logger;
        private readonly IUserSession _userSession;

        public DatabaseInitializer(
            ApplicationDbContext context,
            PersistedGrantDbContext persistedGrantContext,
            ConfigurationDbContext configurationContext,
            ILogger<DatabaseInitializer> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUserSession userSession)
        {
            _persistedGrantContext = persistedGrantContext;
            _configurationContext = configurationContext;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _userSession = userSession;
        }

        public virtual async Task SeedAsync()
        {
            //Apply EF Core migration scripts
            await MigrateAsync();

            //Seed users and roles
            await SeedASPIdentityCoreAsync();

            //Seed clients and Api
            await SeedIdentityServerAsync();

            //Seed blazorboilerplate data
            await SeedBlazorBoilerplateAsync();
        }

        private async Task MigrateAsync()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);
            await _persistedGrantContext.Database.MigrateAsync().ConfigureAwait(false);
            await _configurationContext.Database.MigrateAsync().ConfigureAwait(false);
        }

        private async Task SeedASPIdentityCoreAsync()
        {
            if (!await _context.Tenants.AnyAsync(t => t.Title == TenantConstants.RootTenantTitle))
            {
                _context.Tenants.Add(new Tenant { Title = TenantConstants.RootTenantTitle });
                await _context.SaveChangesAsync();
            }
            _userSession.TenantId = (await _context.Tenants.FirstOrDefaultAsync(t => t.Title == TenantConstants.RootTenantTitle)).Id;

            await EnsureRoleAsync(RoleConstants.AdminRoleName, "Default administrator", ApplicationPermissions.GetAllPermissionValues());
            await EnsureRoleAsync(RoleConstants.UserRoleName, "Default user", new string[] { });
            await EnsureRoleAsync(RoleConstants.TenantManagerRoleName, "Tenant Manager",
                new string[] {
                        Permissions.Tenant.Manager,
                        Permissions.Role.Create,
                        Permissions.Role.Read,
                        Permissions.Role.Update,
                        Permissions.Role.Delete
                });
            if (!await _context.Users.AnyAsync())
            {
                //Generating inbuilt accounts

                await CreateUserAsync("admin", "admin123", "Admin", "Blazor", "Administrator", "admin@blazoreboilerplate.com", "+1 (123) 456-7890", new string[] { RoleConstants.AdminRoleName });
                var user = await CreateUserAsync("user", "user123", "User", "Blazor", "User Blazor", "user@blazoreboilerplate.com", "+1 (123) 456-7890`", new string[] { RoleConstants.UserRoleName });

                _logger.LogInformation("Inbuilt account generation completed");
            }
        }

        private async Task SeedBlazorBoilerplateAsync()
        {
            ApplicationUser user = await _userManager.FindByNameAsync("user");

            if (!_context.UserProfiles.Any())
            {
                UserProfile userProfile = new UserProfile
                {
                    UserId = user.Id,
                    ApplicationUser = user,
                    LastUpdatedDate = DateTime.Now
                };
                _context.UserProfiles.Add(userProfile);
            }

            if (!_context.Todos.Any())
            {
                _context.Todos.AddRange(
                        new Todo
                        {
                            IsCompleted = false,
                            Title = "Test Blazor Boilerplate"
                        },
                        new Todo
                        {
                            IsCompleted = false,
                            Title = "Test Blazor Boilerplate 1",
                        }
                );
            }

            if (!_context.ApiLogs.Any())
            {
                _context.ApiLogs.AddRange(
                new ApiLogItem
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IPAddress = "::1",
                    ApplicationUserId = user.Id
                },
                new ApiLogItem
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IPAddress = "::1",
                    ApplicationUserId = user.Id
                }
            );
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedIdentityServerAsync()
        {
            if (!await _configurationContext.Clients.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Clients");
                foreach (var client in IdentityServerConfig.GetClients())
                {
                    _configurationContext.Clients.Add(client.ToEntity());
                }
                _configurationContext.SaveChanges();
            }
            if (!await _configurationContext.IdentityResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Identity Resources");
                foreach (var resource in IdentityServerConfig.GetIdentityResources())
                {
                    _configurationContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationContext.SaveChanges();
            }
            if (!await _configurationContext.ApiResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Resources");
                foreach (var resource in IdentityServerConfig.GetApiResources())
                {
                    _configurationContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationContext.SaveChanges();
            }
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

        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string firstName, string fullName, string lastName, string email, string phoneNumber, string[] roles)
        {
            var applicationUser = _userManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                applicationUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    FullName = fullName,
                    FirstName = firstName,
                    LastName = lastName
                };

                var result = _userManager.CreateAsync(applicationUser, password).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = _userManager.AddClaimsAsync(applicationUser, new Claim[]{
                        new Claim(JwtClaimTypes.Name, userName),
                        new Claim(JwtClaimTypes.GivenName, firstName),
                        new Claim(JwtClaimTypes.FamilyName, lastName),
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber)
                    }).Result;

                //add claims version of roles
                foreach (var role in roles.Distinct())
                {
                    await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", "true"));
                }

                ApplicationUser user = await _userManager.FindByNameAsync(applicationUser.UserName);

                try
                {
                    result = await _userManager.AddToRolesAsync(user, roles.Distinct());
                }
                catch
                {
                    await _userManager.DeleteAsync(user);
                    throw;
                }

                if (!result.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return applicationUser;
        }
    }
}