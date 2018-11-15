using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common;
using Environment = Domain0.Api.Client.Environment;

namespace Domain0.Desktop.ViewModels
{
    public class ManageMessagesViewModel : BaseManageItemsViewModel<MessageTemplateViewModel, MessageTemplate>
    {
        public static ReactiveList<string> SupportedMessageTypes { get; set; }
            = new ReactiveList<string>(new[] {"sms", "email"});
        public static ReactiveList<string> SupportedMessageLocales { get; set; }
            = new ReactiveList<string>(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.Name));

        public ManageMessagesViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
            : base(shell, domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            _domain0.Model.Environments.Connect()
                .Sort(SortExpressionComparer<Environment>.Ascending(t => t.Id), SortOptimisations.ComparesImmutableValuesOnly, 25)
                .ObserveOnDispatcher()
                .Bind(out _environments)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);

            _environmentsCache = _domain0.Model.Environments
                .AsObservableCache()
                .DisposeWith(Disposables);
        }

        private IObservableCache<Environment, int> _environmentsCache;
        private ReadOnlyObservableCollection<Environment> _environments;
        public ReadOnlyObservableCollection<Environment> Environments => _environments;

        protected override MessageTemplateViewModel TransformToViewModel(MessageTemplate model)
        {
            var vm = base.TransformToViewModel(model);

            _environmentsCache
                .Connect()
                .Select(_ => _environmentsCache.Lookup(vm.EnvironmentId).ValueOrDefault()?.Name)
                .Subscribe(x => vm.Environment = x)
                .DisposeWith(vm.Disposables);

            return vm;
        }

        protected override Task UpdateApi(MessageTemplate m)
        {
            return _domain0.Client.UpdateMessageTemplateAsync(m);
        }

        protected override Task<int> CreateApi(MessageTemplate m)
        {
            return _domain0.Client.CreateMessageTemplateAsync(m);
        }

        protected override Task RemoveApi(int id)
        {
            return _domain0.Client.RemoveMessageTemplateAsync(id);
        }

        protected override ISourceCache<MessageTemplate, int> Models => _domain0.Model.MessageTemplates;
    }
}