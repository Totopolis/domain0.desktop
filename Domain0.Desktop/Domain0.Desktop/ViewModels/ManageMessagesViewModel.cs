using AutoMapper;
using Domain0.Api.Client;
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
    public class ManageMessagesViewModel : ViewModelBase
    {
        private readonly IDomain0Service _domain0;
        private readonly IMapper _mapper;

        public ManageMessagesViewModel(IDomain0Service domain0, IMapper mapper)
        {
            _domain0 = domain0;
            _mapper = mapper;

            Messages.Changed.Subscribe(async x =>
            {
                switch (x.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in x.NewItems)
                            await CreateMessageTemplate(item as MessageTemplateViewModel);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in x.OldItems)
                            await RemoveMessageTemplate(item as MessageTemplateViewModel);
                        break;
                }
            });

            Messages.ItemChanged
                .GroupByUntil(
                    item => item.Sender.Id,
                    item => item.Sender,
                    group => group.Throttle(TimeSpan.FromSeconds(1)))
                .SelectMany(group => group.TakeLast(1))
                .Subscribe(async msg => await UpdateMessageTemplate(msg));

            Refresh();
        }

        public ReactiveList<MessageTemplateViewModel> Messages { get; set; } = new ReactiveList<MessageTemplateViewModel> {ChangeTrackingEnabled = true};

        private async Task Refresh()
        {
            IsBusy = true;
            try
            {
                List<MessageTemplate> result = await _domain0.Client.LoadMessageTemplatesByFilterAsync(new MessageTemplateFilter(new List<int>()));
                Messages.Initialize(_mapper.Map<IEnumerable<MessageTemplateViewModel>>(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            IsBusy = false;
        }

        private async Task UpdateMessageTemplate(MessageTemplateViewModel mt)
        {
            try
            {
                await _domain0.Client.UpdateMessageTemplateAsync(_mapper.Map<MessageTemplate>(mt));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task CreateMessageTemplate(MessageTemplateViewModel mt)
        {
            try
            {
                mt.Id = await _domain0.Client.CreateMessageTemplateAsync(_mapper.Map<MessageTemplate>(mt));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task RemoveMessageTemplate(MessageTemplateViewModel mt)
        {
            try
            {
                await _domain0.Client.RemoveMessageTemplateAsync(mt.Id.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
