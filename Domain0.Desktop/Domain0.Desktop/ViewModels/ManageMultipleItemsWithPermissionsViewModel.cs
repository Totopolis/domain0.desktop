using AutoMapper;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

            PermissionsFilterCommand = ReactiveCommand
                .Create<string>(filter => PermissionsFilter = filter)
                .DisposeWith(Disposables);
            PermissionCheckedCommand = ReactiveCommand
                .Create<SelectedItemPermissionViewModel>(PermissionChecked)
                .DisposeWith(Disposables);

            var permissionsChangedObservable = this.WhenAnyValue(x => x.IsChangedPermissions);
            ApplyPermissionsCommand = ReactiveCommand
                .CreateFromTask(ApplyPermissionsWrapped, permissionsChangedObservable)
                .DisposeWith(Disposables);
            ResetPermissionsCommand = ReactiveCommand
                .CreateFromTask(ResetPermissions, permissionsChangedObservable)
                .DisposeWith(Disposables);
        }

        protected IDisposable SubscribeToPermissions<T>(SourceList<T> source,
            Func<int, IEnumerable<T>, IEnumerable<int>> userIdsSelector)
        {
            var dynamicPermissionsFilter = this
                .WhenAnyValue(x => x.PermissionsFilter)
                .Select(Filters.CreatePermissionsPredicate);

            var sourceList = source
                .Connect()
                .ToCollection();
            var sourcePermissions = _domain0.Model.Permissions
                .Connect()
                .Filter(dynamicPermissionsFilter)
                .ToCollection();
            var sourceSelected = this
                .WhenAnyValue(x => x.SelectedItemsIds);

            return Observable.CombineLatest(
                    sourceList,
                    sourcePermissions,
                    sourceSelected,
                    (itemPermissions, permissions, selectedIds) => selectedIds.Count > 0
                        ? new {itemPermissions, permissions, selectedIds}
                        : null)
                .Throttle(TimeSpan.FromSeconds(.1))
                .Synchronize()
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
                    SelectedItemPermissions = x;
                    IsChangedPermissions = false;
                });
        }

        [Reactive] public string PermissionsFilter { get; set; }
        public ReactiveCommand PermissionsFilterCommand { get; set; }
        public ReactiveCommand PermissionCheckedCommand { get; set; }
        public ReactiveCommand ApplyPermissionsCommand { get; set; }
        public ReactiveCommand ResetPermissionsCommand { get; set; }
        [Reactive] public bool IsChangedPermissions { get; set; }

        [Reactive] public IEnumerable<SelectedItemPermissionViewModel> SelectedItemPermissions { get; set; }

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
