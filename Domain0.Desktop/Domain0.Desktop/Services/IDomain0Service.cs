using System.Threading.Tasks;
using Domain0.Api.Client;
using Domain0.Desktop.Models;

namespace Domain0.Desktop.Services
{
    public interface IDomain0Service
    {
        IDomain0Client Client { get; }
        Task LoadModel();
        void Reconnect();
        Domain0Model Model { get; }
    }
}
