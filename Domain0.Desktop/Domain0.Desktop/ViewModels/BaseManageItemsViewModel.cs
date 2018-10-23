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
using System.Collections.ObjectModel;
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

            OpenCreateFlyoutCommand = ReactiveCommand.Create(() =>
            {
                IsCreateFlyoutOpen = true;
                IsEditFlyoutOpen = false;
            }, this.WhenAny(
                x => x.IsCreateFlyoutOpen,
                x => !x.Value));

            OpenEditFlyoutCommand = ReactiveCommand.Create(() =>
            {
                IsCreateFlyoutOpen = false;
                IsEditFlyoutOpen = true;
            }, OpenEditFlyoutCommandObservable);

            EditSelectedCommand = ReactiveCommand.CreateFromTask(EditSelected);
            CreateCommand = ReactiveCommand.Create(Create);

            DeleteSelectedCommand = ReactiveCommand.CreateFromTask(DeleteSelected, DeleteSelectedCommandObservable);

            UpdateFilters = ReactiveCommand.Create<PropertyFilter>(UpdateFilter);
            ModelFilters = new SourceCache<ModelFilter, PropertyInfo>(x => x.Property);
            var dynamicFilter = ModelFilters.Connect()
                .StartWithEmpty()
                .Bind(out _modelFilters)
                .Select(_ => CreateFilter());

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

        protected virtual IObservable<bool> OpenEditFlyoutCommandObservable =>
            this.WhenAny(
                x => x.IsEditFlyoutOpen,
                x => x.SelectedItem,
                (e, item) => !e.Value && item.Value != null);

        protected virtual IObservable<bool> DeleteSelectedCommandObservable =>
            this.WhenAny(
                x => x.SelectedItem,
                item => item.Value != null);

        private Func<TViewModel, bool> CreateFilter()
        {
            return model =>
            {
                foreach (var filter in _modelFilters)
                {
                    if (string.IsNullOrEmpty(filter.Filter))
                        continue;

                    var value = filter.Property.GetValue(model)?.ToString();
                    if (string.IsNullOrEmpty(value) || !value.Contains(filter.Filter))
                        return false;
                }

                return true;
            };
        }

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

        public ReactiveCommand UpdateFilters { get; set; }
        private SourceCache<ModelFilter, PropertyInfo> ModelFilters { get; }
        private readonly ReadOnlyObservableCollection<ModelFilter> _modelFilters;

        private readonly ReadOnlyObservableCollection<TViewModel> _items;
        public ReadOnlyObservableCollection<TViewModel> Items => _items;

        [Reactive] public TViewModel SelectedItem { get; set; }

        public TViewModel CreateViewModel { get; set; } = new TViewModel();
        public TViewModel EditViewModel { get; set; } = new TViewModel();
        public TModel CreateModel => _mapper.Map<TModel>(CreateViewModel);
        public TModel EditModel => _mapper.Map<TModel>(EditViewModel);

        [Reactive] public bool IsCreateFlyoutOpen { get; set; }
        [Reactive] public bool IsEditFlyoutOpen { get; set; }

        public ReactiveCommand OpenCreateFlyoutCommand { get; set; }
        public ReactiveCommand OpenEditFlyoutCommand { get; set; }

        public ReactiveCommand EditSelectedCommand { get; set; }
        public ReactiveCommand DeleteSelectedCommand { get; set; }
        public ReactiveCommand CreateCommand { get; set; }


        private async Task EditSelected()
        {
            try
            {
                var request = EditModel;
                await UpdateApi(request);
                Models.AddOrUpdate(request);
                IsEditFlyoutOpen = false;
            }
            catch (Exception e)
            {
                _shell.ShowException(e, "Failed to Edit selected");
            }
        }

        private async Task DeleteSelected()
        {
            try
            {
                var id = EditViewModel.Id.Value;
                await RemoveApi(id);
                AfterDeletedSelected(id);
            }
            catch (Exception e)
            {
                _shell.ShowException(e, "Failed to Delete selected");
            }
        }

        protected virtual void AfterDeletedSelected(int id)
        {
            Models.Remove(id);
        }

        private async Task Create()
        {
            try
            {
                CreateViewModel.Id = await CreateApi(CreateModel);
                Models.AddOrUpdate(CreateModel);
                IsCreateFlyoutOpen = false;
            }
            catch (Exception e)
            {
                _shell.ShowException(e, "Failed to Create");
            }
        }

        protected abstract Task UpdateApi(TModel m);
        protected abstract Task<int> CreateApi(TModel m);
        protected abstract Task RemoveApi(int id);

        protected abstract ISourceCache<TModel, int> Models { get; }
    }

    internal class ModelFilter
    {
        public PropertyInfo Property { get; set; }
        public string Filter { get; set; }
    }
}