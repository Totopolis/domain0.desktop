using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;

namespace Domain0.Desktop.ViewModels
{
    public class ManageUsersViewModel : BaseManageItemsViewModel<UserProfileViewModel, UserProfile>
    {
        public ManageUsersViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
            ForceCreateUserCommand = ReactiveCommand.Create(ForceCreateUser);
            ForceCreateUserEmailCommand = ReactiveCommand.Create(ForceCreateUserEmail);
        }

        protected override ISourceCache<UserProfile, int> Models => _domain0.Model.UserProfiles;

        public ReactiveCommand ForceCreateUserCommand { get; set; }
        public ReactiveCommand ForceCreateUserEmailCommand { get; set; }

        [Reactive] public string Phone { get; set; }
        [Reactive] public string Email { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string CustomSmsTemplate { get; set; }
        [Reactive] public string CustomEmailSubjectTemplate { get; set; }
        [Reactive] public string CustomEmailTemplate { get; set; }
        [Reactive] public bool BlockSmsSend { get; set; }
        [Reactive] public bool BlockEmailSend { get; set; }
        
        private async void ForceCreateUser()
        {
            IsBusy = true;
            try
            {
                var phone = long.Parse(Phone);
                var request = new ForceCreateUserRequest(
                    BlockSmsSend, CustomSmsTemplate,
                    Name, phone,
                    new List<string>());
                var userProfile = await _domain0.Client.ForceCreateUserAsync(request);
                OnUserProfileCreated(userProfile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void ForceCreateUserEmail()
        {
            IsBusy = true;
            try
            {
                var request = new ForceCreateEmailUserRequest(
                    BlockEmailSend,
                    CustomEmailSubjectTemplate,
                    CustomEmailTemplate,
                    Email, Name,
                    new List<string>());
                var userProfile = await _domain0.Client.ForceCreateUser2Async(request);
                OnUserProfileCreated(userProfile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnUserProfileCreated(UserProfile userProfile)
        {
            var vm = _mapper.Map<UserProfileViewModel>(userProfile);
            Models.AddOrUpdate(userProfile);
        }

        // BaseManageItemsViewModel
        /*
        protected override IEnumerable<UserProfile> GetItemsFromModel()
        {
            return _domain0.Model.UserProfiles.Values;
        }

        protected override async Task ApiUpdateItemAsync(UserProfile m)
        {
            await _domain0.Client.UpdateUserAsync(m.Id, m);
        }

        protected override Task<int> ApiCreateItemAsync(UserProfile m)
        {
            throw new System.NotImplementedException();
        }

        protected override Task ApiRemoveItemAsync(int id)
        {
            throw new System.NotImplementedException();
        }
        */
    }
}
