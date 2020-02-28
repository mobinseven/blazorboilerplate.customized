using System;
using BlazorBoilerplate.Server.Data.Interfaces;
using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole<Guid>, ITenant
{
}