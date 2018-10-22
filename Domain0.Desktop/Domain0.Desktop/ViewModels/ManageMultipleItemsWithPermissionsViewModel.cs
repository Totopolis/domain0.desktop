using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels
{
    public abstract class ManageMultipleItemsWithPermissionsViewModel<TViewModel, TModel> : ManageMultipleItemsViewModel<TViewModel, TModel> where TViewModel : IItemViewModel, new()
    {
        protected ManageMultipleItemsWithPermissionsViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            PermissionCheckedCommand = ReactiveCommand.Create<SelectedItemPermissionViewModel>(PermissionChecked);

            _domain0.Model.Permissions.Connect()
                .Sort(SortExpressionComparer<Permission>.Ascending(x => x.Id))
                .Bind(out _permissions)
                .Subscribe()
                .DisposeWith(Disposables);
        }

        protected IDisposable SubscribeToPermissions<T>(SourceList<T> source, Func<T, bool> filter, Func<int, IEnumerable<T>, IEnumerable<int>> userIdsSelector)
        {
            return source
                .Connect(filter)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Permissions.Connect().QueryWhenChanged(items => items),
                    this.WhenAnyValue(x => x.SelectedItemsIds),
                    (itemPermissions, permissions, selectedIds) =>
                    {
                        if (selectedIds.Count == 0)
                            return null;

                        return Permissions
                            .Select(p =>
                            {
                                var userIds = userIdsSelector(p.Id.Value, itemPermissions);
                                var groupSelectedIds = selectedIds
                                    .Intersect(userIds)
                                    .ToList();
                                var count = groupSelectedIds.Count;
                                var total = selectedIds.Count;
                                return new SelectedItemPermissionViewModel(count == total, count, total)
                                {
                                    Item = p,
                                    ParentIds = groupSelectedIds
                                };
                            })
                            .OrderByDescending(x => x.Count);
                    })
                .Subscribe(x => SelectedItemPermissions = x);
        }


        public ReactiveCommand PermissionCheckedCommand { get; set; }

        private ReadOnlyObservableCollection<Permission> _permissions;
        public ReadOnlyObservableCollection<Permission> Permissions => _permissions;

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
        }
    }
}
