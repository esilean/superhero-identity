using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Application.Interfaces.Social;
using Application.User.Social;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.Security.Social
{
    public class FacebookAccessor : IFacebookAccessor
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<FacebookAppSettings> _config;
        public FacebookAccessor(IOptions<FacebookAppSettings> config)
        {
            _config = config;
            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri("https://graph.facebook.com/")
            };
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FacebookUserInfo> FacebookLogin(string accessToken)
        {
            var verifyToken = await _httpClient
                .GetAsync($"debug_token?input_token={accessToken}&access_token={_config.Value.AppId}|{_config.Value.AppSecret}");

            if (!verifyToken.IsSuccessStatusCode)
                return null;

            var result = await GetAsync<FacebookUserInfo>(accessToken, "me", "fields=name,email,picture.width(100).height(100)");

            return result;
        }

        private async Task<T> GetAsync<T>(string accessToken, string endPoint, string args)
        {
            var response = await _httpClient.GetAsync($"{endPoint}?access_token={accessToken}&{args}");

            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}