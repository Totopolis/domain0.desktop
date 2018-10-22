using System;
using AutoMapper;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using ReactiveUI;

namespace Domain0.Desktop.ViewModels
{
    public abstract class ManageMultipleItemsViewModel<TViewModel, TModel> : BaseManageItemsViewModel<TViewModel, TModel> where TViewModel : IItemViewModel, new()
    {
        protected ManageMultipleItemsViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
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
                x => x.IsEditFlyoutOpen,
                x => x.SelectedItem,
                x => x.SelectedItemsIds,
                (e, item, items) => !e.Value && item.Value != null && (items.Value == null || items.Value.Count == 1));

        protected override IObservable<bool> DeleteSelectedCommandObservable =>
            this.WhenAny(
                x => x.SelectedItem,
                x => x.SelectedItemsIds,
                (item, items) => item.Value != null && (items.Value == null || items.Value.Count == 1));
        /*
        protected static Dictionary<int, HashSet<int>> SelectedItemsToParents<T>(IEnumerable<SelectedItemViewModel<T>> src)
        {
            var dst = new Dictionary<int, HashSet<int>>();
            foreach (var x in src)
            {
                foreach (var id in x.ParentIds)
                    if (dst.ContainsKey(id))
                        dst[id].Add(x.Id);
                    else
                        dst[id] = new HashSet<int>(new[] { x.Id });
            }
            return dst;
        }
        */
    }
}
