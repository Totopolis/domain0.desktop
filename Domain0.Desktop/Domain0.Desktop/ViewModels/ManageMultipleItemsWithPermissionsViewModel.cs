using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ui.Wpf.Common;

namespace Domain0.Desktop.ViewModels
{
    public abstract class ManageMultipleItemsWithPermissionsViewModel<TViewModel, TModel> : ManageMultipleItemsViewModel<TViewModel, TModel> where TViewModel : IItemViewModel, new()
    {
        protected ManageMultipleItemsWithPermissionsViewModel(IShell shell, IDomain0Service domain0, IMapper mapper) : base(shell, domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            PermissionsFilterCommand = ReactiveCommand.Create<string>(filter =>
            {
                PermissionsFilter = filter;
                UpdateSelectedItemPermissions();
            });
            PermissionCheckedCommand = ReactiveCommand.Create<SelectedItemPermissionViewModel>(PermissionChecked);
            var permissionsChangedObservable = this.WhenAnyValue(x => x.IsChangedPermissions);
            ApplyPermissionsCommand = ReactiveCommand.CreateFromTask(ApplyPermissionsWrapped, permissionsChangedObservable);
            ResetPermissionsCommand = ReactiveCommand.CreateFromTask(ResetPermissions, permissionsChangedObservable);

            _domain0.Model.Permissions.Connect()
                .Sort(SortExpressionComparer<Permission>.Ascending(x => x.Id))
                .ObserveOnDispatcher()
                .Bind(out _permissions)
                .Subscribe()
                .DisposeWith(Disposables);
        }

        protected IDisposable SubscribeToPermissions<T>(SourceList<T> source,
            Func<int, IEnumerable<T>, IEnumerable<int>> userIdsSelector)
        {
            var locker = new object();

            var sourceList = source
                .Connect()
                .ToCollection()
                .Synchronize(locker);
            var sourcePermissions = _domain0.Model.Permissions
                .Connect()
                .QueryWhenChanged(items => items)
                .Synchronize(locker);
            var sourceSelected = this
                .WhenAnyValue(x => x.SelectedItemsIds)
                .Synchronize(locker);

            return Observable.CombineLatest(
                    sourceList,
                    sourcePermissions,
                    sourceSelected,
                    (itemPermissions, permissions, selectedIds) => selectedIds.Count > 0
                        ? new {itemPermissions, permissions = permissions.Items, selectedIds}
                        : null)
                .Throttle(TimeSpan.FromSeconds(.1))
                .Select(o => o?.permissions
                    .Select(p =>
                    {
                        var userIds = userIdsSelector(p.Id.Value, o.itemPermissions);
                        var groupSelectedIds = o.selectedIds
                            .Intersect(userIds)
                            .ToList();
                        var count = groupSelectedIds.Count;
                        var total = o.selectedIds.Count;
                        return new SelectedItemPermissionViewModel(count, total)
                        {
                            Item = p,
                            ParentIds = groupSelectedIds
                        };
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Item.Name)
                    .ThenBy(x => x.Id)
                    .ToList())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    SelectedItemPermissionsRaw = x;
                    UpdateSelectedItemPermissions();
                    IsChangedPermissions = false;
                });
        }

        private void UpdateSelectedItemPermissions()
        {
            SelectedItemPermissions =
                SelectedItemPermissionsRaw?
                    .Where(item => string.IsNullOrEmpty(PermissionsFilter) ||
                                   !string.IsNullOrEmpty(item.Item.Name) &&
                                   item.Item.Name.Contains(PermissionsFilter));
        }

        [Reactive] public string PermissionsFilter { get; set; }
        public ReactiveCommand PermissionsFilterCommand { get; set; }
        public ReactiveCommand PermissionCheckedCommand { get; set; }
        public ReactiveCommand ApplyPermissionsCommand { get; set; }
        public ReactiveCommand ResetPermissionsCommand { get; set; }
        [Reactive] public bool IsChangedPermissions { get; set; }

        private ReadOnlyObservableCollection<Permission> _permissions;
        public ReadOnlyObservableCollection<Permission> Permissions => _permissions;

        [Reactive] public IEnumerable<SelectedItemPermissionViewModel> SelectedItemPermissions { get; set; }
        public IEnumerable<SelectedItemPermissionViewModel> SelectedItemPermissionsRaw { get; set; }

        private void PermissionChecked(SelectedItemPermissionViewModel o)
        {
            if (!o.IsSelected)
            {
                if (o.IsChanged)
                    o.Restore();
                else
                    o.MakeFull();
            }
            else
                o.MakeEmpty();

            IsChangedPermissions = SelectedItemPermissions.Any(x => x.IsChanged);
        }

        private Task ApplyPermissionsWrapped()
        {
            try
            {
                return ApplyPermissions();
            }
            catch (Exception e)
            {
                return _shell.HandleException(e, "Failed to Apply Permissions");
            }
        }

        protected abstract Task ApplyPermissions();
        

        private Task ResetPermissions()
        {
            foreach (var itemPermission in SelectedItemPermissions)
                itemPermission.Restore();
            IsChangedPermissions = false;
            return Task.CompletedTask;
        }
    }
}
