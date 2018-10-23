using System;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using DynamicData;
using ReactiveUI;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Domain0.Desktop.ViewModels.Items;
using Ui.Wpf.Common;

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