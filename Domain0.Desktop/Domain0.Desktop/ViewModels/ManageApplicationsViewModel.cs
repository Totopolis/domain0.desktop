using System;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using DynamicData;
using System.Threading.Tasks;
using Domain0.Desktop.ViewModels.Items;

namespace Domain0.Desktop.ViewModels
{
    public class ManageApplicationsViewModel : BaseManageItemsViewModel<ApplicationViewModel, Application>
    {
        public ManageApplicationsViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override async Task UpdateApi(Application m)
        {
            await _domain0.Client.UpdateApplicationAsync(m);
        }

        protected override async Task<int> CreateApi(Application m)
        {
            return await _domain0.Client.CreateApplicationAsync(m);
        }

        protected override async Task RemoveApi(int id)
        {
            await _domain0.Client.RemoveApplicationAsync(id);
        }

        protected override ISourceCache<Application, int> Models => _domain0.Model.Applications;
    }
}