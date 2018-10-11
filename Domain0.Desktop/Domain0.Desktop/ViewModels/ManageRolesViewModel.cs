﻿using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManageRolesViewModel : BaseManageItemsViewModel<RoleViewModel, Role>
    {
        public ManageRolesViewModel(IDomain0Service domain0, IMapper mapper) : base(domain0, mapper)
        {
        }

        // BaseManageItemsViewModel

        protected override IEnumerable<Role> GetItemsFromModel()
        {
            return _domain0.Model.Roles.Values;
        }

        protected override async Task ApiUpdateItemAsync(Role m)
        {
            await _domain0.Client.UpdateRoleAsync(m);
        }

        protected override async Task<int> ApiCreateItemAsync(Role m)
        {
            return await _domain0.Client.CreateRoleAsync(m);
        }

        protected override async Task ApiRemoveItemAsync(int id)
        {
            await _domain0.Client.RemoveRoleAsync(id);
        }
    }
}