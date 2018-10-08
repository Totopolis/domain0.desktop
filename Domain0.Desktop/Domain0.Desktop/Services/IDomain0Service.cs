using System.Threading.Tasks;
using Domain0.Api.Client;

namespace Domain0.Desktop.Services
{
    public interface IDomain0Service
    {
        Task<bool> Login(string host, string phone, string password);

        IDomain0Client Client { get; }
    }
}
