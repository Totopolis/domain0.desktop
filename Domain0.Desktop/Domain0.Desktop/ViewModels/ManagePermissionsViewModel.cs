using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using DynamicData;
using System.Threading.Tasks;
using DynamicData.Binding;

namespace Domain0.Desktop.ViewModels
{
    public class ManagePermissionsViewModel : BaseManageItemsViewModel<PermissionViewModel, Permission>
    {
        public ManagePermissionsViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
            _domain0.Model.Applications.Connect()
                .Sort(SortExpressionComparer<Application>.Ascending(t => t.Id), SortOptimisations.ComparesImmutableValuesOnly, 25)
                .ObserveOnDispatcher()
                .Bind(out _applications)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);
        }

        private readonly ReadOnlyObservableCollection<Application> _applications;
        public ReadOnlyObservableCollection<Application> Applications => _applications;

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

        protected override Func<Permission, IComparable> ModelComparer => m => m.Id;

        protected override ISourceCache<Permission, int> Models => _domain0.Model.Permissions;
    }
}