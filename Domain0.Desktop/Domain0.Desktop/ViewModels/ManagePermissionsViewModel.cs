using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;

namespace Domain0.Desktop.ViewModels
{
    public class ManagePermissionsViewModel : BaseManageItemsViewModel<PermissionViewModel, Permission>
    {
        public ManagePermissionsViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        protected override ISourceCache<Permission, int> Models => _domain0.Model.Permissions;

        // BaseManageItemsViewModel
        /*
        protected override IEnumerable<Permission> GetItemsFromModel()
        {
            return _domain0.Model.Permissions.Values;
        }

        protected override async Task ApiUpdateItemAsync(Permission m)
        {
            await _domain0.Client.UpdatePermissionAsync(m);
        }

        protected override async Task<int> ApiCreateItemAsync(Permission m)
        {
            return await _domain0.Client.CreatePermissionAsync(m);
        }

        protected override async Task ApiRemoveItemAsync(int id)
        {
            await _domain0.Client.RemovePermissionAsync(id);
        }
        */
    }
}