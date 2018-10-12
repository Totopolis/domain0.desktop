using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;

namespace Domain0.Desktop.ViewModels
{
    public class ManageMessagesViewModel : BaseManageItemsViewModel<MessageTemplateViewModel, MessageTemplate>
    {
        public static ReactiveList<string> SupportedMessageTypes { get; set; }
            = new ReactiveList<string>(new[] {"sms", "email"});
        public static ReactiveList<string> SupportedMessageLocales { get; set; }
            = new ReactiveList<string>(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.Name));

        public ManageMessagesViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override ISourceCache<MessageTemplate, int> Models => _domain0.Model.MessageTemplates;

        // BaseManageItemsViewModel
        /*
        protected override IEnumerable<MessageTemplate> GetItemsFromModel()
        {
            return _domain0.Model.MessageTemplates.Values;
        }

        protected override async Task ApiUpdateItemAsync(MessageTemplate m)
        {
            await _domain0.Client.UpdateMessageTemplateAsync(m);
        }

        protected override async Task<int> ApiCreateItemAsync(MessageTemplate m)
        {
            return await _domain0.Client.CreateMessageTemplateAsync(m);
        }

        protected override async Task ApiRemoveItemAsync(int id)
        {
            await _domain0.Client.RemoveMessageTemplateAsync(id);
        }
        */
    }
}