using AutoMapper;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common.ViewModels;

namespace Domain0.Desktop.ViewModels
{
    public abstract class BaseManageItemsViewModel<TViewModel, TModel> : ViewModelBase
        where TViewModel : IItemViewModel
    {
        protected readonly IDomain0Service _domain0;
        protected readonly IMapper _mapper;

        public BaseManageItemsViewModel(IDomain0Service domain0, IMapper mapper)
        {
            _domain0 = domain0;
            _mapper = mapper;

            Items.Changed.Subscribe(async x =>
            {
                switch (x.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in x.NewItems)
                            await CreateItem((TViewModel)item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in x.OldItems)
                            await RemoveItem((TViewModel)item);
                        break;
                }
            });

            Items.ItemChanged
                .GroupByUntil(
                    item => item.Sender.Id,
                    item => item.Sender,
                    group => group.Throttle(TimeSpan.FromSeconds(1)))
                .SelectMany(group => group.TakeLast(1))
                .Subscribe(async x => await UpdateItem(x));

            Refresh();
        }

        public ReactiveList<TViewModel> Items { get; set; } = new ReactiveList<TViewModel> { ChangeTrackingEnabled = true };


        public virtual async Task Refresh()
        {
            IsBusy = true;
            try
            {
                List<TModel> result = await ApiLoadItemsAsync();
                Items.Initialize(_mapper.Map<IEnumerable<TViewModel>>(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            IsBusy = false;
        }


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

        protected abstract Task<List<TModel>> ApiLoadItemsAsync();
        protected abstract Task ApiUpdateItemAsync(TModel m);
        protected abstract Task<int> ApiCreateItemAsync(TModel m);
        protected abstract Task ApiRemoveItemAsync(int id);
    }
}
