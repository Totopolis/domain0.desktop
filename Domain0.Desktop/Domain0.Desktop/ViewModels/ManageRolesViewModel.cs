using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManageRolesViewModel : BaseManageItemsViewModel<RoleViewModel, Role>
    {
        public ManageRolesViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            _permissions = _domain0.Model.Permissions
                .AsObservableCache()
                .DisposeWith(Disposables);
        }

        private IObservableCache<Permission, int> _permissions;

        protected override RoleViewModel TransformToViewModel(Role model)
        {
            var vm = base.TransformToViewModel(model);

            var permissionsChanges = _permissions.Connect();
            var rolePermissions = _domain0.Model.RolePermissions
                .Connect()
                .Filter(x => x.RoleId == vm.Id)
                .ToCollection();

            var combined = Observable.CombineLatest(
                rolePermissions,
                permissionsChanges,
                (rp, _) => rp.Select(x => _permissions.Lookup(x.Id.Value).Value)
            );

            combined
                .Subscribe(x =>
                {
                    vm.Permissions = x;
                    vm.PermissionsString = string.Join(",", x.Select(p => p.Name));
                })
                .DisposeWith(vm.Disposables);

            return vm;
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

        protected override void AfterDeletedSelected(int id)
        {
            _domain0.Model.UserPermissions.Edit(innerList =>
            {
                var userPermissions = innerList.Where(x => x.RoleId == id);
                innerList.RemoveMany(userPermissions);
            });
            _domain0.Model.RolePermissions.Edit(innerList =>
            {
                var rolePermissions = innerList.Where(x => x.RoleId == id);
                innerList.RemoveMany(rolePermissions);
            });

            base.AfterDeletedSelected(id);
        }

        protected override ISourceCache<Role, int> Models => _domain0.Model.Roles;
    }
}