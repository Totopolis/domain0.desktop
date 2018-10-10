using Domain0.Api.Client;
using Domain0.Desktop.Properties;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Domain0.Desktop.Services
{
    public class Domain0Service : IDomain0Service
    {
        private readonly HttpClient _httpClient;
        private readonly Domain0Client _client;

        public Domain0Service()
        {
            _httpClient = new HttpClient();
            _client = new Domain0Client(null, _httpClient);
        }

        public string HostUrl
        {
            get => _client.BaseUrl;
            set => _client.BaseUrl = value;
        }

        public bool LoadToken()
        {
            var savedToken = Settings.Default.AccessToken;

            if (string.IsNullOrEmpty(savedToken))
                return false;

            try
            {
                var token = AccessTokenResponse.FromJson(savedToken);
                AccessToken = token;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void ResetAccessToken()
        {
            Settings.Default.AccessToken = string.Empty;
            Settings.Default.Save();

            AccessToken = null;
        }

        public void UpdateAccessToken(AccessTokenResponse token, bool shouldRemember)
        {
            if (shouldRemember)
            {
                Settings.Default.AccessToken = token.ToJson();
                Settings.Default.Save();
            }
            else
            {
                Settings.Default.AccessToken = string.Empty;
                Settings.Default.Save();
            }

            AccessToken = token;
        }


        public IDomain0Client Client => _client;


        private AccessTokenResponse AccessToken
        {
            set => _httpClient.DefaultRequestHeaders.Authorization =
                value != null ? new AuthenticationHeaderValue("Bearer", value.AccessToken) : null;
        }
    }
}