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
    public class ManageUsersViewModel : ManageMultipleItemsWithPermissionsViewModel<UserProfileViewModel, UserProfile>
    {
        public ManageUsersViewModel(IDomain0Service domain0, IMapper mapper)
            : base(domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            ForceCreateUserRolesFilterCommand = ReactiveCommand.Create<string>(x => ForceCreateUserRolesFilter = x);
            ForceCreateUserCommand = ReactiveCommand.Create(ForceCreateUser);
            
            LockUsersCommand = ReactiveCommand.CreateFromTask<IList>(LockUsers,
                this.WhenAny(x => x.SelectedItemsIds, x => x.Value != null && x.Value.Count > 0));

            RoleCheckedCommand = ReactiveCommand.Create<SelectedUserRoleViewModel>(RoleChecked);
            var rolesChangedObservable = this.WhenAnyValue(x => x.IsChangedRoles);
            ApplyRolesCommand = ReactiveCommand.CreateFromTask(ApplyRoles, rolesChangedObservable);
            ResetRolesCommand = ReactiveCommand.CreateFromTask(ResetRoles, rolesChangedObservable);
            

            // Permissions

            SubscribeToPermissions(
                    _domain0.Model.UserPermissions,
                    x => !x.RoleId.HasValue,
                    (permissionId, items) => items
                        .Where(x => x.Id.Value == permissionId)
                        .Select(x => x.UserId))
                .DisposeWith(Disposables);

            // Roles

            var dynamicForceCreateUserRolesFilter =
                this.WhenAnyValue(x => x.ForceCreateUserRolesFilter)
                    .Select(CreateForceCreateUserRolesFilter);

            _domain0.Model.Roles.Connect()
                .Filter(dynamicForceCreateUserRolesFilter)
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

            _domain0.Model.UserPermissions
                .Connect(x => x.RoleId.HasValue)
                .ToCollection()
                .CombineLatest(
                    _domain0.Model.Roles.Connect().QueryWhenChanged(items => items),
                    this.WhenAnyValue(x => x.SelectedItemsIds),
                    (userPermissions, roles, selectedIds) =>
                    {
                        if (selectedIds.Count == 0)
                            return null;

                        return Roles
                            .Select(r =>
                            {
                                var roleId = r.Id.Value;
                                var userIds = userPermissions
                                    .Where(x => x.RoleId.Value == roleId)
                                    .Select(x => x.UserId)
                                    .Distinct();
                                var groupSelectedIds = selectedIds
                                    .Intersect(userIds)
                                    .ToList();
                                var count = groupSelectedIds.Count;
                                var total = selectedIds.Count;
                                return new SelectedUserRoleViewModel(count == total, count, total)
                                {
                                    Item = r,
                                    ParentIds = groupSelectedIds
                                };
                            })
                            .OrderByDescending(x => x.Count)
                            .ToList();
                    })
                .Subscribe(x =>
                {
                    SelectedUserRoles = x;
                    IsChangedRoles = false;
                })
                .DisposeWith(Disposables);
        }

        private Func<Role, bool> CreateForceCreateUserRolesFilter(string x)
        {
            if (string.IsNullOrEmpty(x))
                return role => true;

            return role => !string.IsNullOrEmpty(role.Name) &&
                           role.Name.Contains(x);
        }

        private ReadOnlyObservableCollection<Role> _roles;
        public ReadOnlyObservableCollection<Role> Roles => _roles;

        private ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> _forceCreateUserRoles;
        public ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> ForceCreateUserRoles => _forceCreateUserRoles;

        [Reactive] public IEnumerable<SelectedUserRoleViewModel> SelectedUserRoles {get; set; }
        public ReactiveCommand RoleCheckedCommand { get; set; }
        public ReactiveCommand ApplyRolesCommand { get; set; }
        public ReactiveCommand ResetRolesCommand { get; set; }
        [Reactive] public bool IsChangedRoles { get; set; }

        public ReactiveCommand LockUsersCommand { get; set; }

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

        private void RoleChecked(SelectedUserRoleViewModel o)
        {
            if (!o.IsSelected)
            {
                if (o.IsChanged)
                    o.Restore();
                else
                    o.MakeFull();
            }
            else
                o.MakeEmpty();

            IsChangedRoles = SelectedUserRoles.Any(x => x.IsChanged);
        }

        private Task ApplyRoles()
        {
            foreach (var userRole in SelectedUserRoles)
                userRole.Restore();
            IsChangedRoles = false;
            return Task.CompletedTask;
        }

        private Task ResetRoles()
        {
            foreach (var userRole in SelectedUserRoles)
                userRole.Restore();
            IsChangedRoles = false;
            return Task.CompletedTask;
        }

        /*
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
        */
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

        [Reactive] public string ForceCreateUserRolesFilter { get; set; }
        public ReactiveCommand ForceCreateUserRolesFilterCommand { get; set; }
        public ReactiveCommand ForceCreateUserCommand { get; set; }
        
        public int ForceCreateUserMode { get; set; }
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
                var userProfile = await ForceCreateUserApi();
                Models.AddOrUpdate(userProfile);
                IsCreateFlyoutOpen = false;
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

        private async Task<UserProfile> ForceCreateUserApi()
        {
            var roles = ForceCreateUserRoles
                .Where(x => x.IsSelected)
                .Select(x => x.Role.Name)
                .ToList();

            switch ((ForceCreateUserModeEnum)ForceCreateUserMode)
            {
                case ForceCreateUserModeEnum.Phone:
                    var phone = long.Parse(Phone);
                    var requestByPhone = new ForceCreateUserRequest(
                        BlockSmsSend, CustomSmsTemplate,
                        Name, phone,
                        roles);
                    return await _domain0.Client.ForceCreateUserAsync(requestByPhone);
                case ForceCreateUserModeEnum.Email:
                    var requestByEmail = new ForceCreateEmailUserRequest(
                        BlockEmailSend,
                        CustomEmailSubjectTemplate,
                        CustomEmailTemplate,
                        Email, Name,
                        roles);
                    return await _domain0.Client.ForceCreateUser2Async(requestByEmail);
                default:
                    throw new ArgumentException();
            }
        }
    }

    public enum ForceCreateUserModeEnum
    {
        Phone,
        Email
    }
}
