using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Data
{
    public static class ChangeTrackerExtensions
    {
        public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession userSession)
        {
            changeTracker.DetectChanges();
            ApplicationDbContext dbContext = (ApplicationDbContext)changeTracker.Context;
            Guid userId = Guid.Empty;
            var timestamp = DateTime.UtcNow;

            if (userSession.UserId != Guid.Empty)
            {
                userId = userSession.UserId;
            }
            var entries = changeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                //Auditable Entity Model
                if (entry.Entity is IAuditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                        entry.Property("CreatedBy").CurrentValue = userId;
                    }

                    if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;
                        entry.Property("ModifiedBy").CurrentValue = userId;
                    }
                }

                if (entry.Entity is ITenant)
                {
                    Guid TenantId = Guid.Empty;
                    //read tenantId from userSession, else use root tenant.
                    if (userSession.TenantId == Guid.Empty)
                    {
                        TenantId = dbContext.Tenants.Where(t => t.Title == TenantConstants.RootTenantTitle).FirstOrDefault().Id;
                    }
                    else
                    {
                        TenantId = userSession.TenantId;
                    }
                    entry.Property("TenantId").CurrentValue = TenantId;
                }

                //Soft Delete Entity Model
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }
        }
    }
}