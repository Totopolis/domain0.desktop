using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using System.Threading.Tasks;
using Ui.Wpf.Common;

namespace Domain0.Desktop.ViewModels
{
    public class ManageApplicationsViewModel : BaseManageItemsViewModel<ApplicationViewModel, Application>
    {
        public ManageApplicationsViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
            : base(shell, domain0, mapper)
        {
        }

        protected override Task UpdateApi(Application m)
        {
            return _domain0.Client.UpdateApplicationAsync(m);
        }

        protected override Task<int> CreateApi(Application m)
        {
            return _domain0.Client.CreateApplicationAsync(m);
        }

        protected override Task RemoveApi(int id)
        {
            return _domain0.Client.RemoveApplicationAsync(id);
        }

        protected override ISourceCache<Application, int> Models => _domain0.Model.Applications;
    }
}