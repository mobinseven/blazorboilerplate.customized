using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Services.Implementations
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public AuthorizeApi(NavigationManager navigationManager, HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponseDto> Login(LoginDto loginParameters)
        {
            ApiResponseDto resp;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Account/Login")
            {
                Content = new StringContent(JsonConvert.SerializeObject(loginParameters))
            };
            httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage))
            {
                response.EnsureSuccessStatusCode();

#if ServerSideBlazor

                if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> cookieEntries))
                {
                    Uri uri = response.RequestMessage.RequestUri;
                    CookieContainer cookieContainer = new CookieContainer();

                    foreach (string cookieEntry in cookieEntries)
                    {
                        cookieContainer.SetCookies(uri, cookieEntry);
                    }

                    IEnumerable<Cookie> cookies = cookieContainer.GetCookies(uri).Cast<Cookie>();

                    foreach (Cookie cookie in cookies)
                    {
                        await _jsRuntime.InvokeVoidAsync("jsInterops.setCookie", cookie.ToString());
                    }
                }
#endif

                string content = await response.Content.ReadAsStringAsync();
                resp = JsonConvert.DeserializeObject<ApiResponseDto>(content);
            }

            return resp;
        }

        public async Task<ApiResponseDto> Logout()
        {
#if ServerSideBlazor
            List<string> cookies = _httpClient.DefaultRequestHeaders?.GetValues("Cookie")?.ToList();
#endif

            var resp = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);

#if ServerSideBlazor
            if (resp.StatusCode == 200 && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

                foreach (string cookie in cookies[0].Split(';'))
                {
                    string[] cookieParts = cookie.Split('=');
                    await _jsRuntime.InvokeVoidAsync("jsInterops.removeCookie", cookieParts[0]);
                }
            }
#endif

            return resp;
        }

        public async Task<ApiResponseDto> Create(RegisterDto registerParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Create", registerParameters);
        }

        public async Task<ApiResponseDto> Register(RegisterDto registerParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Register", registerParameters);
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto confirmEmailParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ConfirmEmail", confirmEmailParameters);
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto resetPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ResetPassword", resetPasswordParameters);
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ForgotPassword", forgotPasswordParameters);
        }

        public async Task<UserInfoDto> GetUserInfo()
        {
            UserInfoDto userInfo = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Account/UserInfo");

            if (apiResponse.StatusCode == 200)
            {
                userInfo = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
                return userInfo;
            }
            return userInfo;
        }

        public async Task<UserInfoDto> GetUser()
        {
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Account/GetUser");
            UserInfoDto user = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
            return user;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userInfo);
        }
    }
}