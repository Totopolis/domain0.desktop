using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain0.Desktop.ViewModels.Items;

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

        protected override async Task UpdateApi(UserProfile m)
        {
            await _domain0.Client.UpdateUserAsync(m.Id, m);
        }

        protected override Task<int> CreateApi(UserProfile m)
        {
            throw new NotImplementedException();
        }

        protected override async Task RemoveApi(int id)
        {
            var userProfile = Models.Lookup(id).Value;
            await _domain0.Client.DeleteUserAsync((long)userProfile.Phone.Value);
        }

        protected override void AfterDeletedSelected(int id)
        {
            _domain0.Model.UserPermissions.Edit(innerList =>
            {
                var userPermissions = innerList.Where(x => x.UserId == id);
                innerList.RemoveMany(userPermissions);
            });

            base.AfterDeletedSelected(id);
        }

        protected override ISourceCache<UserProfile, int> Models => _domain0.Model.UserProfiles;
        
        public ReactiveCommand ForceCreateUserCommand { get; set; }
        public ReactiveCommand ForceCreateUserEmailCommand { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string CustomSmsTemplate { get; set; }
        public string CustomEmailSubjectTemplate { get; set; }
        public string CustomEmailTemplate { get; set; }
        public bool BlockSmsSend { get; set; }
        public bool BlockEmailSend { get; set; }
        
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
            Models.AddOrUpdate(userProfile);
        }
    }
}
