using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

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

            LockUsersCommand = ReactiveCommand.CreateFromTask<IList>(LockUsers,
                this.WhenAny(x => x.SelectedItemsIds, x => x.Value != null && x.Value.Count > 0));

            AddRoleCommand = ReactiveCommand.CreateFromTask<Role>(AddRole);
            RemoveRolesCommand = ReactiveCommand.CreateFromTask<IList>(RemoveRoles);
            AddPermissionCommand = ReactiveCommand.CreateFromTask<Permission>(AddPermission);
            RemovePermissionsCommand = ReactiveCommand.CreateFromTask<IList>(RemovePermissions);

            // Permissions

            _domain0.Model.Permissions.Connect()
                .Sort(SortExpressionComparer<Permission>.Ascending(x => x.Id))
                .Bind(out _permissions)
                .Subscribe()
                .DisposeWith(Disposables);

            var permissionsDynamicFilter = this
                .WhenAnyValue(x => x.SelectedItemsIds)
                .Select(CreatePermissionsFilter);
                //.ObserveOn(RxApp.MainThreadScheduler);

            _domain0.Model.UserPermissions
                .Connect()
                .Filter(permissionsDynamicFilter, ListFilterPolicy.ClearAndReplace)
                .GroupWithImmutableState(x => x.Id.Value)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Permissions.Connect().QueryWhenChanged(items => items),
                    (groups, permissions) => groups
                        .Select(group => new SelectedUserPermissionViewModel
                        {
                            Id = group.Key,
                            Permission = permissions.Lookup(group.Key).Value,
                            Count = group.Items.Count(),
                            Total = SelectedItemsIds?.Count ?? 0,
                            ParentIds = group.Items.Select(x => x.UserId)
                        })
                        .OrderByDescending(x => x.Count)
                )
                .Subscribe(x => SelectedUserPermissions = x)
                .DisposeWith(Disposables);

            // Roles

            _domain0.Model.Roles.Connect()
                .Transform(x => new ForceCreateUserRoleViewModel { Role = x })
                .Sort(SortExpressionComparer<ForceCreateUserRoleViewModel>.Ascending(x => x.Role.Id))
                .Bind(out _forceCreateUserRoles)
                .Subscribe()
                .DisposeWith(Disposables);

            _domain0.Model.Roles.Connect()
                .Sort(SortExpressionComparer<Role>.Ascending(x => x.Id))
                .Bind(out _roles)
                .Subscribe()
                .DisposeWith(Disposables);

            var rolesDynamicFilter = this
                .WhenAnyValue(x => x.SelectedItemsIds)
                .Select(CreateRolesFilter);
                //.ObserveOn(RxApp.MainThreadScheduler);

            _domain0.Model.UserPermissions
                .Connect()
                .Filter(rolesDynamicFilter, ListFilterPolicy.ClearAndReplace)
                .GroupWithImmutableState(x => x.RoleId.Value)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Roles.Connect().QueryWhenChanged(items => items),
                    (groups, roles) => groups
                        .Select(g =>
                        {
                            var result = new SelectedUserRoleViewModel
                            {
                                Id = g.Key,
                                Role = roles.Lookup(g.Key).Value,
                                Count = g.Items
                                    .Select(x => x.UserId)
                                    .Distinct()
                                    .Count(),
                                Total = SelectedItemsIds?.Count ?? 0,
                                ParentIds = g.Items
                                    .Select(x => x.UserId)
                                    .Distinct()
                            };
                            return result;
                        })
                        .OrderByDescending(x => x.Count)
                )
                .Subscribe(x => SelectedUserRoles = x)
                .DisposeWith(Disposables);
        }

        private Func<UserPermission, bool> CreatePermissionsFilter(ICollection<int> x)
        {
            if (x == null)
                return permission => false;

            return permission => x.Contains(permission.UserId) &&
                                 !permission.RoleId.HasValue;
        }

        private Func<UserPermission, bool> CreateRolesFilter(ICollection<int> x)
        {
            if (x == null)
                return permission => false;

            return permission => x.Contains(permission.UserId) &&
                                 permission.RoleId.HasValue;
        }

        private ReadOnlyObservableCollection<Permission> _permissions;
        public ReadOnlyObservableCollection<Permission> Permissions => _permissions;

        private ReadOnlyObservableCollection<Role> _roles;
        public ReadOnlyObservableCollection<Role> Roles => _roles;

        private ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> _forceCreateUserRoles;
        public ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> ForceCreateUserRoles => _forceCreateUserRoles;

        [Reactive] public IEnumerable<SelectedUserPermissionViewModel> SelectedUserPermissions { get; set; }
        [Reactive] public IEnumerable<SelectedUserRoleViewModel> SelectedUserRoles {get; set; }

        public ReactiveCommand AddRoleCommand { get; set; }
        public ReactiveCommand RemoveRolesCommand { get; set; }
        public ReactiveCommand AddPermissionCommand { get; set; }
        public ReactiveCommand RemovePermissionsCommand { get; set; }

        public ReactiveCommand LockUsersCommand { get; set; }

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
            await _domain0.Client.DeleteUserAsync(id);
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

        // Manage Roles and Permissions

        private async Task AddRole(Role role)
        {
            if (SelectedItemsIds == null || SelectedItemsIds.Count == 0)
                return;

            var roleId = role.Id.Value;
            var userIds = SelectedItemsIds.ToList();
            var vm = SelectedUserRoles?.FirstOrDefault(x => x.Id == roleId);
            var permissions =
                _domain0.Model.RolePermissions.Items
                    .Where(x => x.RoleId == roleId)
                    .ToList();

            foreach (var userId in userIds)
            {
                if (vm == null || !vm.ParentIds.Contains(userId))
                {
                    await _domain0.Client.AddUserRolesAsync(userId,
                        new IdArrayRequest(new List<int> {roleId}));

                    _domain0.Model.UserPermissions.AddRange(
                        permissions.Select(p =>
                            new UserPermission(p.ApplicationId, p.Description,
                                p.Id.Value, p.Name, roleId, userId)));
                }
            }
        }

        private async Task RemoveRoles(IList list)
        {
            var src = list.Cast<SelectedUserRoleViewModel>();
            var dst = SelectedItemsToParents(src);
            foreach (var kv in dst)
            {
                await _domain0.Client.RemoveUserRolesAsync(kv.Key, new IdArrayRequest(kv.Value.ToList()));
                _domain0.Model.UserPermissions.Edit(innerList =>
                {
                    var toRemove = innerList.Where(x =>
                        x.UserId == kv.Key &&
                        x.RoleId.HasValue &&
                        kv.Value.Contains(x.RoleId.Value)
                    ).ToList();
                    innerList.RemoveMany(toRemove);
                });
            }
        }

        private async Task AddPermission(Permission permission)
        {
            if (SelectedItemsIds == null || SelectedItemsIds.Count == 0)
                return;

            var permissionId = permission.Id.Value;
            var userIds = SelectedItemsIds.ToList();
            var vm = SelectedUserPermissions?.FirstOrDefault(x => x.Id == permissionId);

            foreach (var userId in userIds)
            {
                if (vm == null || !vm.ParentIds.Contains(userId))
                {
                    await _domain0.Client.AddUserPermissionsAsync(userId,
                        new IdArrayRequest(new List<int> {permissionId}));

                    _domain0.Model.UserPermissions.Add(
                        new UserPermission(permission.ApplicationId, permission.Description,
                            permissionId, permission.Name, null, userId));
                }
            }
        }

        private async Task RemovePermissions(IList list)
        {
            var src = list.Cast<SelectedUserPermissionViewModel>();
            var dst = SelectedItemsToParents(src);
            foreach (var kv in dst)
            {
                await _domain0.Client.RemoveUserPermissionsAsync(kv.Key, new IdArrayRequest(kv.Value.ToList()));
                _domain0.Model.UserPermissions.Edit(innerList =>
                {
                    var toRemove = innerList.Where(x =>
                        x.UserId == kv.Key &&
                        !x.RoleId.HasValue &&
                        kv.Value.Contains(x.Id.Value)
                    );
                    innerList.RemoveMany(toRemove);
                });
            }
        }

        // Lock

        private async Task LockUsers(IList list)
        {
            var users = list.Cast<UserProfileViewModel>();

            var toLock = users.Any(x => !x.IsLocked);
            foreach (var user in users)
            {
                if (toLock != user.IsLocked)
                {
                    if (toLock)
                        await _domain0.Client.LockUserAsync(user.Id.Value);
                    else
                        await _domain0.Client.UnlockUserAsync(user.Id.Value);

                    user.IsLocked = toLock;
                }
            }
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
            IsCreateFlyoutOpen = false;
        }
    }
}
