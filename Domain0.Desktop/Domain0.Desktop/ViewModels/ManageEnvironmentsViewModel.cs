using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using System.Threading.Tasks;
using Ui.Wpf.Common;

namespace Domain0.Desktop.ViewModels
{
    public class ManageEnvironmentsViewModel : BaseManageItemsViewModel<EnvironmentViewModel, Environment>
    {
        public ManageEnvironmentsViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
            : base(shell, domain0, mapper)
        {
        }

        protected override Task UpdateApi(Environment m)
        {
            return _domain0.Client.UpdateEnvironmentAsync(m);
        }

        protected override Task<int> CreateApi(Environment m)
        {
            return _domain0.Client.CreateEnvironmentAsync(m);
        }

        protected override Task RemoveApi(int id)
        {
            return _domain0.Client.RemoveEnvironmentAsync(id);
        }

        protected override ISourceCache<Environment, int> Models => _domain0.Model.Environments;
    }
}