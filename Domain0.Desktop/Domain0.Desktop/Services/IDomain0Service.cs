using System.Threading.Tasks;
using Domain0.Api.Client;

namespace Domain0.Desktop.Services
{
    public interface IDomain0Service
    {
        string HostUrl { get; set; }

        bool LoadToken();
        void ResetAccessToken();
        void UpdateAccessToken(AccessTokenResponse token, bool shouldRemember);

        Task<AccessTokenResponse> Login(string phone, string password);

        IDomain0Client Client { get; }
    }
}
