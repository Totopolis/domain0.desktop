using AutoMapper;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using Domain0.Desktop.Views.Converters;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace Domain0.Desktop.ViewModels
{
    public abstract class BaseManageItemsViewModel<TViewModel, TModel> : ViewModelBase where TViewModel : IItemViewModel, new()
    {
        protected readonly IShell _shell;
        protected readonly IDomain0Service _domain0;
        protected readonly IMapper _mapper;

        protected BaseManageItemsViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
        {
            _shell = shell;
            _domain0 = domain0;
            _mapper = mapper;

            OpenCreateFlyoutCommand = ReactiveCommand
                .Create(() =>
                {
                    IsCreateFlyoutOpen = !IsCreateFlyoutOpen;
                    IsEditFlyoutOpen = false;
                }, OpenCreateFlyoutCommandObservable)
                .DisposeWith(Disposables);

            OpenEditFlyoutCommand = ReactiveCommand
                .Create(() =>
                {
                    IsCreateFlyoutOpen = false;
                    IsEditFlyoutOpen = !IsEditFlyoutOpen;
                }, OpenEditFlyoutCommandObservable)
                .DisposeWith(Disposables);

            EditSelectedCommand = ReactiveCommand
                .CreateFromTask(EditSelected)
                .DisposeWith(Disposables);
            CreateCommand = ReactiveCommand
                .CreateFromTask(Create)
                .DisposeWith(Disposables);

            CreatedItemInList = new Interaction<TViewModel, Unit>();

            DeleteSelectedCommand = ReactiveCommand
                .CreateFromTask(DeleteSelected, DeleteSelectedCommandObservable)
                .DisposeWith(Disposables);

            UpdateFilters = ReactiveCommand
                .Create<PropertyFilter>(UpdateFilter)
                .DisposeWith(Disposables);

            ModelFilters = new SourceCache<ModelFilter, PropertyInfo>(x => x.Property)
                .DisposeWith(Disposables);

            var dynamicFilter = ModelFilters.Connect()
                .StartWithEmpty()
                .ToCollection()
                .Select(Filters.CreateModelFilter<TViewModel>);

            Initialize();

            Models.Connect()
                .Transform(TransformToViewModel)
                .Filter(dynamicFilter)
                .Sort(SortExpressionComparer<TViewModel>.Ascending(x => x.Id), SortOptimisations.ComparesImmutableValuesOnly, 25)
                .ObserveOnDispatcher()
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);

            this.WhenAnyValue(x => x.SelectedItem)
                .Subscribe(SelectionChanged)
                .DisposeWith(Disposables);
        }

        protected virtual void Initialize()
        {

        }

        protected virtual IObservable<bool> OpenCreateFlyoutCommandObservable => null;

        protected virtual IObservable<bool> OpenEditFlyoutCommandObservable =>
            this.WhenHaveSelectedItem();

        protected virtual IObservable<bool> DeleteSelectedCommandObservable =>
            this.WhenHaveSelectedItem();

        private void UpdateFilter(PropertyFilter filter)
        {
            if (filter == null)
                return;

            ModelFilters.AddOrUpdate(new ModelFilter
            {
                Filter = filter.Filter,
                Property = typeof(TViewModel).GetProperty(filter.Name)
            });
        }

        private void SelectionChanged(TViewModel item)
        {
            if (item != null)
                _mapper.Map(item, EditViewModel);
            IsEditFlyoutOpen = false;
        }

        protected virtual TViewModel TransformToViewModel(TModel model)
        {
            return _mapper.Map<TViewModel>(model);
        }

        public ReactiveCommand<PropertyFilter, Unit> UpdateFilters { get; set; }
        private SourceCache<ModelFilter, PropertyInfo> ModelFilters { get; }

        private readonly ReadOnlyObservableCollection<TViewModel> _items;
        public ReadOnlyObservableCollection<TViewModel> Items => _items;

        [Reactive] public TViewModel SelectedItem { get; set; }

        public TViewModel CreateViewModel { get; set; } = new TViewModel();
        public TViewModel EditViewModel { get; set; } = new TViewModel();
        public TModel CreateModel => _mapper.Map<TModel>(CreateViewModel);
        public TModel EditModel => _mapper.Map<TModel>(EditViewModel);

        [Reactive] public bool IsCreateFlyoutOpen { get; set; }
        [Reactive] public bool IsEditFlyoutOpen { get; set; }

        public ReactiveCommand<Unit, Unit> OpenCreateFlyoutCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenEditFlyoutCommand { get; set; }

        public ReactiveCommand<Unit, Unit> EditSelectedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateCommand { get; set; }

        public Interaction<TViewModel, Unit> CreatedItemInList { get; set; }

        protected IDisposable HandleInteractionOnCreatedItemInList(int id)
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => ((INotifyCollectionChanged) Items).CollectionChanged += h,
                    h => ((INotifyCollectionChanged) Items).CollectionChanged -= h)
                .Select(x =>
                    x.EventArgs.NewItems
                        .Cast<TViewModel>()
                        .FirstOrDefault(newItem => newItem.Id.Value == id))
                .Where(x => x != null)
                .Take(1)
                .Subscribe(async x =>
                {
                    try
                    {
                        await CreatedItemInList.Handle(x);
                    }
                    catch
                    {
                        // ignored
                    }
                });
        }

        private async Task EditSelected()
        {
            try
            {
                Trace.TraceInformation("Edit {0}: {1}", typeof(TModel).Name, EditViewModel.Id);
                var request = EditModel;
                await UpdateApi(request);
                Models.AddOrUpdate(request);
                IsEditFlyoutOpen = false;
            }
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Edit selected");
            }
        }

        private async Task DeleteSelected()
        {
            try
            {
                Trace.TraceInformation("Delete {0}: {1}", typeof(TModel).Name, EditViewModel.Id);
                var id = EditViewModel.Id.Value;
                await RemoveApi(id);
                AfterDeletedSelected(id);
            }
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Delete selected");
            }
        }

        protected virtual void AfterDeletedSelected(int id)
        {
            Models.Remove(id);
        }

        protected virtual async Task Create()
        {
            var createdItemSubscription = Disposable.Empty;
            try
            {
                var id = await CreateApi(CreateModel);
                CreateViewModel.Id = id;
                createdItemSubscription = HandleInteractionOnCreatedItemInList(id);
                Models.AddOrUpdate(CreateModel);

                Trace.TraceInformation("Created {0}: {1}", typeof(TModel).Name, CreateViewModel.Id);

                IsCreateFlyoutOpen = false;
            }
            catch (Exception e)
            {
                createdItemSubscription.Dispose();
                await _shell.HandleException(e, "Failed to Create");
            }
        }

        protected abstract Task UpdateApi(TModel m);
        protected abstract Task<int> CreateApi(TModel m);
        protected abstract Task RemoveApi(int id);

        protected abstract ISourceCache<TModel, int> Models { get; }


        protected static void TraceApplied(string title, Dictionary<int, HashSet<int>> toAdd, Dictionary<int, HashSet<int>> toRemove)
        {
            var added = string.Join(",", toAdd.Select(x => $"[{x.Key}:{string.Join(",", x.Value)}]"));
            var removed = string.Join(",", toRemove.Select(x => $"[{x.Key}:{string.Join(",", x.Value)}]"));
            Trace.TraceInformation($"Applied {title}:" + 
                                   (!string.IsNullOrEmpty(added) ? $" ++{added}" : "") +
                                   (!string.IsNullOrEmpty(removed) ? $" --{removed}" : ""));
        }

    }
}