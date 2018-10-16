using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;

namespace Domain0.Desktop.ViewModels
{
    public class ManageUsersViewModel : BaseManageItemsViewModel<UserProfileViewModel, UserProfile>
    {
        public ManageUsersViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            ForceCreateUserCommand = ReactiveCommand.Create(ForceCreateUser);
            ForceCreateUserEmailCommand = ReactiveCommand.Create(ForceCreateUserEmail);

            // Permissions

            _domain0.Model.Permissions.Connect()
                .Sort(SortExpressionComparer<Permission>.Ascending(x => x.Id))
                .Bind(out _permissions)
                .Subscribe()
                .DisposeWith(Disposables);

            var permissionsDynamicFilter = this
                .WhenAnyValue(x => x.SelectedItem)
                .Select(CreatePermissionsFilter);

            _domain0.Model.UserPermissions
                .Connect()
                .Filter(permissionsDynamicFilter)
                .QueryWhenChanged(items => items.Select(x => x.Id.Value))
                .CombineLatest(
                    _domain0.Model.Permissions.Connect().QueryWhenChanged(items => items),
                    (ids, permissions) => ids.Select(id => permissions.Lookup(id).Value)
                )
                .Subscribe(x => SelectedUserPermissions = x)
                .DisposeWith(Disposables);

            // Roles

            _domain0.Model.Roles.Connect()
                .Sort(SortExpressionComparer<Role>.Ascending(x => x.Id))
                .Bind(out _roles)
                .Subscribe()
                .DisposeWith(Disposables);

            var rolesDynamicFilter = this
                .WhenAnyValue(x => x.SelectedItem)
                .Select(CreateRolesFilter);

            _domain0.Model.UserPermissions
                .Connect()
                .Filter(rolesDynamicFilter)
                .DistinctValues(x => x.RoleId.Value)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Roles.Connect().QueryWhenChanged(items => items),
                    (ids, roles) => ids.Select(id => roles.Lookup(id).Value)
                )
                .Subscribe(x => SelectedUserRoles = x)
                .DisposeWith(Disposables);
        }

        private Func<UserPermission, bool> CreatePermissionsFilter(UserProfileViewModel x)
        {
            if (x == null)
                return permission => false;

            return permission => permission.UserId == x.Id &&
                                 !permission.RoleId.HasValue;
        }

        private Func<UserPermission, bool> CreateRolesFilter(UserProfileViewModel x)
        {
            if (x == null)
                return permission => false;

            return permission => permission.UserId == x.Id &&
                                 permission.RoleId.HasValue;
        }

        private ReadOnlyObservableCollection<Permission> _permissions;
        public ReadOnlyObservableCollection<Permission> Permissions => _permissions;

        private ReadOnlyObservableCollection<Role> _roles;
        public ReadOnlyObservableCollection<Role> Roles => _roles;

        [Reactive] public IEnumerable<Permission> SelectedUserPermissions { get; set; }
        [Reactive] public IEnumerable<Role> SelectedUserRoles {get; set; }

        public ReactiveCommand ForceCreateUserCommand { get; set; }
        public ReactiveCommand ForceCreateUserEmailCommand { get; set; }

        protected override ISourceCache<UserProfile, int> Models => _domain0.Model.UserProfiles;

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

        // Creation

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
