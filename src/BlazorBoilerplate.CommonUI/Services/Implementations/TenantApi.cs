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
    public class TenantApi : ITenantApi
    {
        public TenantDto Tenant { get; set; } = new TenantDto();
        public BlazorBoilerplate.Shared.AuthorizationDefinitions.TenantRole TenantRole { get; set; }
        private readonly IAuthorizeApi _authorizeApi;

        public TenantApi(IAuthorizeApi authorizeApi)
        {
            _authorizeApi = authorizeApi;
        }

        public async Task<TenantDto> GetUserTenant()
        {
            //ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Tenants/GetUserTenant");
            //if (apiResponse.Result != null)
            //{
            //    Tenant = JsonConvert.DeserializeObject<TenantDto>(apiResponse.Result.ToString());
            //}
            UserInfoDto userInfo = await _authorizeApi.GetUser();
            bool IsAuthenticated = userInfo.IsAuthenticated;
            if (IsAuthenticated)
            {
                userInfo = await _authorizeApi.GetUserInfo();
                Tenant.Id = TenantAuthorization.ExtractTenantId(userInfo.ExposedClaims.Find(c => c.Key == TenantAuthorization.TenantClaimType).Value);
            }

            return Tenant;
        }
    }
}