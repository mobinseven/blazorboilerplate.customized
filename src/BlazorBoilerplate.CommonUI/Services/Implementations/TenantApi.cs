using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Services.Implementations
{
    public class TenantApi : ITenantApi
    {
        public TenantDto Tenant { get; set; }
        public BlazorBoilerplate.Shared.AuthorizationDefinitions.TenantRole TenantRole { get; set; }
        private readonly HttpClient _httpClient;

        public TenantApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TenantDto> GetUserTenant()
        {
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Tenants/GetUserTenant");
            if (apiResponse.Result != null)
            {
                Tenant = JsonConvert.DeserializeObject<TenantDto>(apiResponse.Result.ToString());
            }

            return Tenant;
        }
    }
}