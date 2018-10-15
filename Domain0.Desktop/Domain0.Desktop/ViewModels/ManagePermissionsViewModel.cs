using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManagePermissionsViewModel : BaseManageItemsViewModel<PermissionViewModel, Permission>
    {
        public ManagePermissionsViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            _domain0.Model.Applications.Connect()
                .Sort(SortExpressionComparer<Application>.Ascending(t => t.Id), SortOptimisations.ComparesImmutableValuesOnly, 25)
                .ObserveOnDispatcher()
                .Bind(out _applications)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);

            _applicationsCache = _domain0.Model.Applications
                .AsObservableCache()
                .DisposeWith(Disposables);
        }

        private IObservableCache<Application, int> _applicationsCache;
        private ReadOnlyObservableCollection<Application> _applications;
        public ReadOnlyObservableCollection<Application> Applications => _applications;

        protected override PermissionViewModel TransformToViewModel(Permission model)
        {
            var vm = base.TransformToViewModel(model);

            _applicationsCache
                .Connect()
                .Select(x => _applicationsCache.Lookup(vm.ApplicationId).Value.Name)
                .Subscribe(x => vm.Application = x)
                .DisposeWith(vm.Disposables);
            
            return vm;
        }

        protected override async Task UpdateApi(Permission m)
        {
            await _domain0.Client.UpdatePermissionAsync(m);
        }

        protected override async Task<int> CreateApi(Permission m)
        {
            return await _domain0.Client.CreatePermissionAsync(m);
        }

        protected override async Task RemoveApi(int id)
        {
            await _domain0.Client.RemovePermissionAsync(id);
        }

        protected override void AfterDeletedSelected(int id)
        {
            _domain0.Model.UserPermissions.Edit(innerList =>
            {
                var userPermissions = innerList.Where(x => x.Id == id);
                innerList.RemoveMany(userPermissions);
            });
            _domain0.Model.RolePermissions.Edit(innerList =>
            {
                var rolePermissions = innerList.Where(x => x.Id == id);
                innerList.RemoveMany(rolePermissions);
            });

            base.AfterDeletedSelected(id);
        }

        protected override ISourceCache<Permission, int> Models => _domain0.Model.Permissions;
    }
}