using System;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole<Guid>, ITenant
{
    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public ApplicationRole() : base()
    {
    }
}