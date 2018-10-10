using Domain0.Api.Client;

namespace Domain0.Desktop.Services
{
    public interface IDomain0Service
    {
        string HostUrl { get; set; }

        bool LoadToken();
        void ResetAccessToken();
        void UpdateAccessToken(AccessTokenResponse token, bool shouldRemember);
        
        IDomain0Client Client { get; }
    }
}
