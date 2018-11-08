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
using Ui.Wpf.Common;

namespace Domain0.Desktop.ViewModels
{
    public class ManageRolesViewModel : ManageMultipleItemsWithPermissionsViewModel<RoleViewModel, Role>
    {
        public ManageRolesViewModel(IShell shell, IDomain0Service domain0, IMapper mapper) : base(shell, domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            _permissionsCache = _domain0.Model.Permissions
                .AsObservableCache()
                .DisposeWith(Disposables);

            SubscribeToPermissions(
                    _domain0.Model.RolePermissions,
                    (permissionId, items) => items
                        .Where(x => x.Id.Value == permissionId)
                        .Select(x => x.RoleId))
                .DisposeWith(Disposables);
        }

        private IObservableCache<Permission, int> _permissionsCache;

        protected override RoleViewModel TransformToViewModel(Role model)
        {
            var vm = base.TransformToViewModel(model);

            var permissionsChanges = _permissionsCache.Connect();
            var rolePermissions = _domain0.Model.RolePermissions
                .Connect()
                .ToCollection();

            var combined = Observable.CombineLatest(
                rolePermissions,
                permissionsChanges,
                (rp, _) => rp
                    .Where(x => x.RoleId == vm.Id)
                    .Select(x => _permissionsCache.Lookup(x.Id.Value).Value)
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

        protected override async Task ApplyPermissions()
        {
            var (toAdd, toRemove) = GetItemsChanges(SelectedItemPermissions, SelectedItemsIds);

            foreach (var rKV in toRemove)
                await _domain0.Client.RemoveRolePermissionsAsync(rKV.Key,
                    new IdArrayRequest(rKV.Value.ToList()));

            var rpToRemove = toRemove.SelectMany(x =>
            {
                return _domain0.Model.RolePermissions.Items
                    .Where(up => up.RoleId == x.Key && x.Value.Contains(up.Id.Value));
            }).ToList();

            foreach (var aKV in toAdd)
                await _domain0.Client.AddRolePermissionsAsync(aKV.Key,
                    new IdArrayRequest(aKV.Value.ToList()));

            var rpToAdd = toAdd.SelectMany(x =>
            {
                return _domain0.Model.Permissions.Items
                    .Where(p => x.Value.Contains(p.Id.Value))
                    .Select(p => new RolePermission(p.ApplicationId, p.Description, p.Id, p.Name, x.Key));
            }).ToList();

            _domain0.Model.RolePermissions.Edit(innerList =>
            {
                innerList.RemoveMany(rpToRemove);
                innerList.AddRange(rpToAdd);
            });

            TraceApplied("Permissions to Roles", toAdd, toRemove);
        }
        
        protected override Task UpdateApi(Role m)
        {
            return _domain0.Client.UpdateRoleAsync(m);
        }

        protected override Task<int> CreateApi(Role m)
        {
            return _domain0.Client.CreateRoleAsync(m);
        }

        protected override Task RemoveApi(int id)
        {
            return _domain0.Client.RemoveRoleAsync(id);
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