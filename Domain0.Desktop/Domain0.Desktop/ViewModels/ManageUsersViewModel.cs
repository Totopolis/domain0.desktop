using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain0.Desktop.ViewModels
{
    public class ManageUsersViewModel : BaseManageItemsViewModel<UserProfileViewModel, UserProfile>
    {
        public ManageUsersViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override async Task<List<UserProfile>> ApiLoadItemsAsync()
        {
            var filter = new UserProfileFilter(new List<int>());
            return await _domain0.Client.GetUserByFilterAsync(filter);
        }

        protected override async Task ApiUpdateItemAsync(UserProfile m)
        {
            await _domain0.Client.UpdateUserAsync(m);
        }

        protected override Task<int> ApiCreateItemAsync(UserProfile m)
        {
            throw new System.NotImplementedException();
        }

        protected override Task ApiRemoveItemAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
