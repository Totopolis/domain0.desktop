using System;
using AutoMapper;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using Ui.Wpf.Common;

namespace Domain0.Desktop.ViewModels
{
    public abstract class ManageMultipleItemsViewModel<TViewModel, TModel> : BaseManageItemsViewModel<TViewModel, TModel> where TViewModel : IItemViewModel, new()
    {
        protected ManageMultipleItemsViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
            : base(shell, domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            SelectedItemsIds = new HashSet<int>();
        }

        [Reactive] public ICollection<int> SelectedItemsIds { get; set; }

        protected override IObservable<bool> OpenEditFlyoutCommandObservable =>
            this.WhenAny(
                x => x.SelectedItem,
                x => x.SelectedItemsIds,
                (item, items) => item.Value != null && (items.Value == null || items.Value.Count == 1));

        protected override IObservable<bool> DeleteSelectedCommandObservable =>
            this.WhenAny(
                x => x.SelectedItem,
                x => x.SelectedItemsIds,
                (item, items) => item.Value != null && (items.Value == null || items.Value.Count == 1));

        protected static void ItemToParents(Dictionary<int, HashSet<int>> dst, int itemId, IEnumerable<int> parents)
        {
            foreach (var id in parents)
                if (dst.ContainsKey(id))
                    dst[id].Add(itemId);
                else
                    dst[id] = new HashSet<int>(new[] { itemId });
        }

        protected static ValueTuple<Dictionary<int, HashSet<int>>, Dictionary<int, HashSet<int>>> GetItemsChanges<T>(
            IEnumerable<SelectedItemViewModel<T>> items, ICollection<int> selectedIds)
        {
            var toAdd = new Dictionary<int, HashSet<int>>();
            var toRemove = new Dictionary<int, HashSet<int>>();
            foreach (var item in items)
            {
                if (!item.IsChanged)
                    continue;

                if (item.IsEmpty)
                    ItemToParents(toRemove, item.Id, item.ParentIds);
                else if (item.IsFull)
                    ItemToParents(toAdd, item.Id, selectedIds.Except(item.ParentIds));
            }

            return (toAdd, toRemove);
        }
    }
}
