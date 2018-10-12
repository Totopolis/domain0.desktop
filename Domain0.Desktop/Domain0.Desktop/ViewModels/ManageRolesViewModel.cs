using System;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using DynamicData;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManageRolesViewModel : BaseManageItemsViewModel<RoleViewModel, Role>
    {
        public ManageRolesViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        protected override async Task UpdateApi(Role m)
        {
            await _domain0.Client.UpdateRoleAsync(m);
        }

        protected override async Task<int> CreateApi(Role m)
        {
            return await _domain0.Client.CreateRoleAsync(m);
        }

        protected override async Task RemoveApi(int id)
        {
            await _domain0.Client.RemoveRoleAsync(id);
        }

        protected override Func<Role, IComparable> ModelComparer => m => m.Id;

        protected override ISourceCache<Role, int> Models => _domain0.Model.Roles;
    }
}