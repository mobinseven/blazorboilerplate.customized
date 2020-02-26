using BlazorBoilerplate.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Services.Contracts
{
    public interface ITenantApi
    {
        Task<TenantDto> GetUserTenant();
    }
}