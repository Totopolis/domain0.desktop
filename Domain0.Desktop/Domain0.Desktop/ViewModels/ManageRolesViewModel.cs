using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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
            _permissionsCache = _domain0.Model.Permissions
                .AsObservableCache()
                .DisposeWith(Disposables);

            AddPermissionCommand = ReactiveCommand.CreateFromTask<Permission>(AddPermission);
            RemovePermissionsCommand = ReactiveCommand.CreateFromTask<IList>(RemovePermissions);

            _domain0.Model.Permissions.Connect()
                .Sort(SortExpressionComparer<Permission>.Ascending(x => x.Id))
                .Bind(out _permissions)
                .Subscribe()
                .DisposeWith(Disposables);

            var permissionsDynamicFilter = this
                .WhenAnyValue(x => x.SelectedItemsIds)
                .Select(CreatePermissionsFilter);
                //.ObserveOn(RxApp.MainThreadScheduler);

            _domain0.Model.RolePermissions
                .Connect()
                .Filter(permissionsDynamicFilter, ListFilterPolicy.ClearAndReplace)
                .GroupWithImmutableState(x => x.Id.Value)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Permissions.Connect().QueryWhenChanged(items => items),
                    (groups, permissions) => groups
                        .Select(group => new SelectedRolePermissionViewModel
                        {
                            Id = group.Key,
                            Permission = permissions.Lookup(group.Key).Value,
                            Count = group.Items.Count(),
                            Total = SelectedItemsIds.Count,
                            ParentIds = group.Items.Select(x => x.RoleId)
                        })
                        .OrderByDescending(x => x.Count)
                )
                .Subscribe(x => SelectedRolePermissions = x)
                .DisposeWith(Disposables);
        }

        private Func<RolePermission, bool> CreatePermissionsFilter(ICollection<int> x)
        {
            if (x == null)
                return permission => false;

            return permission => x.Contains(permission.RoleId);
        }

        private ReadOnlyObservableCollection<Permission> _permissions;
        public ReadOnlyObservableCollection<Permission> Permissions => _permissions;

        [Reactive] public IEnumerable<SelectedRolePermissionViewModel> SelectedRolePermissions { get; set; }

        public ReactiveCommand AddPermissionCommand { get; set; }
        public ReactiveCommand RemovePermissionsCommand { get; set; }

        private IObservableCache<Permission, int> _permissionsCache;

        protected override RoleViewModel TransformToViewModel(Role model)
        {
            var vm = base.TransformToViewModel(model);

            var permissionsChanges = _permissionsCache.Connect();
            var rolePermissions = _domain0.Model.RolePermissions
                .Connect()
                .Filter(x => x.RoleId == vm.Id)
                .ToCollection();

            var combined = Observable.CombineLatest(
                rolePermissions,
                permissionsChanges,
                (rp, _) => rp.Select(x => _permissionsCache.Lookup(x.Id.Value).Value)
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



        private async Task AddPermission(Permission permission)
        {
            var permissionId = permission.Id.Value;
            var roleIds = SelectedItemsIds.ToList();
            var vm = SelectedRolePermissions?.FirstOrDefault(x => x.Id == permissionId);

            foreach (var roleId in roleIds)
            {
                if (vm == null || !vm.ParentIds.Contains(roleId))
                {
                    await _domain0.Client.AddRolePermissionsAsync(roleId,
                        new IdArrayRequest(new List<int> { permissionId }));

                    _domain0.Model.RolePermissions.Add(
                        new RolePermission(permission.ApplicationId, permission.Description,
                            permissionId, permission.Name, roleId));
                }
            }
        }

        private async Task RemovePermissions(IList list)
        {
            var src = list.Cast<SelectedRolePermissionViewModel>();
            var dst = SelectedItemsToParents(src);
            foreach (var kv in dst)
            {
                await _domain0.Client.RemoveRolePermissionsAsync(kv.Key, new IdArrayRequest(kv.Value.ToList()));
                _domain0.Model.RolePermissions.Edit(innerList =>
                {
                    var toRemove = innerList.Where(x =>
                        x.RoleId == kv.Key &&
                        kv.Value.Contains(x.Id.Value)
                    );
                    innerList.RemoveMany(toRemove);
                });
            }
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