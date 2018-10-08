using Domain0.Api.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        public async Task<bool> Login(string host, string phone, string password)
        {
            _client.BaseUrl = host;

            AccessTokenResponse token = await _client.LoginAsync(new SmsLoginRequest(password, phone));

            if (token == null)
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return true;
        }

        public IDomain0Client Client => _client;
    }
}