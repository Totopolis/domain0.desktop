using AutoMapper;
using Domain0.Desktop.Services;
using Domain0.Desktop.Views.Converters;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Ui.Wpf.Common.ViewModels;

namespace Domain0.Desktop.ViewModels
{
    public abstract class BaseManageItemsViewModel<TViewModel, TModel> : ViewModelBase
        where TViewModel : IItemViewModel
    {
        protected readonly IDomain0Service _domain0;
        protected readonly IMapper _mapper;

        protected BaseManageItemsViewModel(IDomain0Service domain0, IMapper mapper)
        {
            _domain0 = domain0;
            _mapper = mapper;

            /*
            Items.Changed.Subscribe(async x =>
                {
                    switch (x.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var item in x.NewItems)
                                await CreateItem((TViewModel) item);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var item in x.OldItems)
                                await RemoveItem((TViewModel) item);
                            break;
                    }
                })
                .DisposeWith(Disposables);
                */

            /*
            Items.ItemChanged
                .GroupByUntil(
                    item => item.Sender.Id,
                    item => item.Sender,
                    group => group.Throttle(TimeSpan.FromSeconds(1)))
                .SelectMany(group => group.TakeLast(1))
                .Subscribe(async x => await UpdateItem(x))
                .DisposeWith(Disposables);
                */

            UpdateFilters = ReactiveCommand.Create<PropertyFilter>(UpdateFilter);
            ModelFilters = new SourceCache<ModelFilter, PropertyInfo>(x => x.Property);
            var dynamicFilter = ModelFilters.Connect()
                .StartWithEmpty()
                .Bind(out _modelFilters)
                .Select(_ => CreateFilter());

            Models.Connect()
                .Filter(dynamicFilter)
                .ObserveOnDispatcher()
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);
        }

        private Func<TModel, bool> CreateFilter()
        {
            return model =>
            {
                foreach (var filter in _modelFilters)
                {
                    if (string.IsNullOrEmpty(filter.Filter))
                        continue;

                    var value = filter.Property.GetValue(model).ToString();
                    if (!value.Contains(filter.Filter))
                        return false;
                }

                return true;
            };
        }

        public ReactiveCommand UpdateFilters { get; set; }
        private SourceCache<ModelFilter, PropertyInfo> ModelFilters { get; }
        private readonly ReadOnlyObservableCollection<ModelFilter> _modelFilters;

        protected abstract ISourceCache<TModel, int> Models { get; }

        protected ReadOnlyObservableCollection<TModel> _items;
        public ReadOnlyObservableCollection<TModel> Items => _items;

        private void UpdateFilter(PropertyFilter filter)
        {
            if (filter == null)
                return;

            ModelFilters.AddOrUpdate(new ModelFilter
            {
                Filter = filter.Filter,
                Property = typeof(TModel).GetProperty(filter.Name)
            });
        }

        /*
        private async Task UpdateItem(TViewModel vm)
        {
            try
            {
                await ApiUpdateItemAsync(_mapper.Map<TModel>(vm));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task CreateItem(TViewModel vm)
        {
            try
            {
                vm.Id = await ApiCreateItemAsync(_mapper.Map<TModel>(vm));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task RemoveItem(TViewModel vm)
        {
            try
            {
                await ApiRemoveItemAsync(vm.Id.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        
        protected abstract Task ApiUpdateItemAsync(TModel m);
        protected abstract Task<int> ApiCreateItemAsync(TModel m);
        protected abstract Task ApiRemoveItemAsync(int id);
        */
    }

    internal class ModelFilter
    {
        public PropertyInfo Property { get; set; }
        public string Filter { get; set; }
    }
}