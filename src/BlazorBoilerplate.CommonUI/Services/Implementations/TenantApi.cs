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
        private readonly HttpClient _httpClient;

        public TenantApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TenantDto> GetUserTenant()
        {
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Tenants/GetUserTenant");
            TenantDto tenant = null;
            if (apiResponse.Result != null)
                tenant = JsonConvert.DeserializeObject<TenantDto>(apiResponse.Result.ToString());
            return tenant;
        }
    }
}