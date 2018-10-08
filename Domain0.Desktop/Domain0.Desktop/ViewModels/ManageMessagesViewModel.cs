using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManageMessagesViewModel : BaseManageItemsViewModel<MessageTemplateViewModel, MessageTemplate>
    {
        public ManageMessagesViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override async Task<List<MessageTemplate>> ApiLoadItemsAsync()
        {
            var filter = new MessageTemplateFilter(new List<int>());
            return await _domain0.Client.LoadMessageTemplatesByFilterAsync(filter);
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
    }
}