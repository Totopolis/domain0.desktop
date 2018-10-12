﻿using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;

namespace Domain0.Desktop.ViewModels
{
    public class ManageApplicationsViewModel : BaseManageItemsViewModel<ApplicationViewModel, Application>
    {
        public ManageApplicationsViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override ISourceCache<Application, int> Models => _domain0.Model.Applications;

        /*
        protected override IEnumerable<Application> GetItemsFromModel()
        {
            return _domain0.Model.Applications.Values;
        }

        protected override async Task ApiUpdateItemAsync(Application m)
        {
            await _domain0.Client.UpdateApplicationAsync(m);
        }

        protected override async Task<int> ApiCreateItemAsync(Application m)
        {
            return await _domain0.Client.CreateApplicationAsync(m);
        }

        protected override async Task ApiRemoveItemAsync(int id)
        {
            await _domain0.Client.RemoveApplicationAsync(id);
        }
        */
    }
}