using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
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
    public class TenantApi
    {
        public TenantDto Tenant { get; set; } = new TenantDto();
        public BlazorBoilerplate.Shared.AuthorizationDefinitions.TenantRole TenantRole { get; set; }
        private readonly IAuthorizeApi _authorizeApi;
        private readonly HttpClient _httpClient;

        public TenantApi(IAuthorizeApi authorizeApi, HttpClient httpClient)
        {
            _authorizeApi = authorizeApi;
            _httpClient = httpClient;
        }

        public async Task<TenantDto> GetUserTenant()
        {
            //UserInfoDto userInfo = await _authorizeApi.GetUser();
            //bool IsAuthenticated = userInfo.IsAuthenticated;
            //if (IsAuthenticated)
            //{
            //    userInfo = await _authorizeApi.GetUserInfo();
            //    Tenant.Id = TenantAuthorization.ExtractTenantId(userInfo.ExposedClaims.Find(c => c.Key == TenantAuthorization.TenantClaimType).Value);
            //}
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Tenants/GetUserTenant");
            if (apiResponse.Result != null)
            {
                Tenant = JsonConvert.DeserializeObject<TenantDto>(apiResponse.Result.ToString());
            }
            return Tenant;
        }
    }
}