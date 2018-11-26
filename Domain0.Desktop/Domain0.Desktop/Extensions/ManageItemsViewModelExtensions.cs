using Domain0.Desktop.ViewModels;
using Domain0.Desktop.ViewModels.Items;
using ReactiveUI;
using System;

namespace Domain0.Desktop.Extensions
{
    public static class ManageItemsViewModelExtensions
    {
        public static IObservable<bool> WhenHaveSelectedItem<TViewModel, TModel>(
            this BaseManageItemsViewModel<TViewModel, TModel> vm)
            where TViewModel : IItemViewModel, new()
        {
            return vm.WhenAny(x => x.SelectedItem, x => x.Value != null);
        }

        public static IObservable<bool> WhenHaveOnlyOneSelectedItem<TViewModel, TModel>(
            this ManageMultipleItemsViewModel<TViewModel, TModel> vm)
            where TViewModel : IItemViewModel, new()
        {
            return vm.WhenAny(
                x => x.SelectedItem,
                x => x.SelectedItemsIds,
                (item, items) => item.Value != null && (items.Value == null || items.Value.Count == 1)
            );
        }
    }
}