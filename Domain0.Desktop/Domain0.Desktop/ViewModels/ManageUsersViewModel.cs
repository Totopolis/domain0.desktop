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
using Domain0.Desktop.Extensions;
using Ui.Wpf.Common;
using UserPermission = Domain0.Api.Client.UserPermission;

namespace Domain0.Desktop.ViewModels
{
    public class ManageUsersViewModel : ManageMultipleItemsWithPermissionsViewModel<UserProfileViewModel, UserProfile>
    {
        public ManageUsersViewModel(IShell shell, IDomain0Service domain0, IMapper mapper)
            : base(shell, domain0, mapper)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            ForceCreateUserRolesFilterCommand = ReactiveCommand.Create<string>(x => ForceCreateUserRolesFilter = x);
            ForceCreateUserCommand = ReactiveCommand.Create(ForceCreateUser);
            
            LockUsersCommand = ReactiveCommand.CreateFromTask<IList>(LockUsers,
                this.WhenAny(x => x.SelectedItemsIds, x => x.Value != null && x.Value.Count > 0));

            RolesFilterCommand = ReactiveCommand.Create<string>(filter =>
            {
                RolesFilter = filter;
                UpdateSelectedUserRoles();
            });
            RoleCheckedCommand = ReactiveCommand.Create<SelectedUserRoleViewModel>(RoleChecked);
            var rolesChangedObservable = this.WhenAnyValue(x => x.IsChangedRoles);
            ApplyRolesCommand = ReactiveCommand.CreateFromTask(ApplyRoles, rolesChangedObservable);
            ResetRolesCommand = ReactiveCommand.CreateFromTask(ResetRoles, rolesChangedObservable);
            
            // Permissions

            SubscribeToPermissions(
                    _domain0.Model.UserPermissions,
                    (permissionId, items) => items
                        .Where(x => !x.RoleId.HasValue && x.Id.Value == permissionId)
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
                .ObserveOnDispatcher()
                .Bind(out _forceCreateUserRoles)
                .Subscribe()
                .DisposeWith(Disposables);

            var locker = new object();
            var sourceUserPermissions = _domain0.Model.UserPermissions
                .Connect()
                .ToCollection()
                .Synchronize(locker);
            var sourceRoles = _domain0.Model.Roles
                .Connect()
                .QueryWhenChanged(items => items)
                .Synchronize(locker);
            var sourceSelected = this
                .WhenAnyValue(x => x.SelectedItemsIds)
                .Synchronize(locker);

            Observable.CombineLatest(
                    sourceUserPermissions,
                    sourceRoles,
                    sourceSelected,
                    (userPermissions, roles, selectedIds) => selectedIds.Count > 0
                        ? new {userPermissions, roles = roles.Items, selectedIds}
                        : null)
                .Throttle(TimeSpan.FromSeconds(.1))
                .Select(o => o?.roles
                    .Select(r =>
                    {
                        var roleId = r.Id.Value;
                        var userIds = o.userPermissions
                            .Where(x => x.RoleId.HasValue && x.RoleId.Value == roleId)
                            .Select(x => x.UserId)
                            .Distinct();
                        var groupSelectedIds = o.selectedIds
                            .Intersect(userIds)
                            .ToList();
                        var count = groupSelectedIds.Count;
                        var total = o.selectedIds.Count;
                        return new SelectedUserRoleViewModel(count, total)
                        {
                            Item = r,
                            ParentIds = groupSelectedIds
                        };
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Item.Name)
                    .ThenBy(x => x.Id)
                    .ToList())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    SelectedUserRolesRaw = x;
                    UpdateSelectedUserRoles();
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

        private ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> _forceCreateUserRoles;
        public ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> ForceCreateUserRoles => _forceCreateUserRoles;

        private void UpdateSelectedUserRoles()
        {
            SelectedUserRoles =
                SelectedUserRolesRaw?
                    .Where(item => string.IsNullOrEmpty(RolesFilter) ||
                                   !string.IsNullOrEmpty(item.Item.Name) &&
                                   item.Item.Name.Contains(RolesFilter));
        }

        [Reactive] public IEnumerable<SelectedUserRoleViewModel> SelectedUserRoles {get; set; }
        public IEnumerable<SelectedUserRoleViewModel> SelectedUserRolesRaw {get; set; }
        [Reactive] public string RolesFilter { get; set; }
        public ReactiveCommand RolesFilterCommand { get; set; }
        public ReactiveCommand RoleCheckedCommand { get; set; }
        public ReactiveCommand ApplyRolesCommand { get; set; }
        public ReactiveCommand ResetRolesCommand { get; set; }
        [Reactive] public bool IsChangedRoles { get; set; }

        public ReactiveCommand LockUsersCommand { get; set; }

        protected override ISourceCache<UserProfile, int> Models => _domain0.Model.UserProfiles;

        protected override Task UpdateApi(UserProfile m)
        {
            return _domain0.Client.UpdateUserAsync(m.Id, m);
        }

        protected override Task<int> CreateApi(UserProfile m)
        {
            throw new NotImplementedException();
        }

        protected override Task RemoveApi(int id)
        {
            return _domain0.Client.DeleteUserAsync(id);
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

        private async Task ApplyRoles()
        {
            try
            {
                var (toAdd, toRemove) = GetItemsChanges(SelectedUserRoles, SelectedItemsIds);

                foreach (var rKV in toRemove)
                    await _domain0.Client.RemoveUserRolesAsync(rKV.Key,
                        new IdArrayRequest(rKV.Value.ToList()));

                var upToRemove = toRemove.SelectMany(x =>
                {
                    return _domain0.Model.UserPermissions.Items
                        .Where(rp => rp.UserId == x.Key && rp.RoleId.HasValue && x.Value.Contains(rp.RoleId.Value));
                }).ToList();

                foreach (var aKV in toAdd)
                    await _domain0.Client.AddUserRolesAsync(aKV.Key,
                        new IdArrayRequest(aKV.Value.ToList()));

                var upToAdd = toAdd.SelectMany(x =>
                {
                    return x.Value
                        .SelectMany(v =>
                        {
                            return _domain0.Model.RolePermissions.Items
                                .Where(rp => rp.RoleId == v)
                                .Select(p =>
                                    new UserPermission(p.ApplicationId, p.Description, p.Id, p.Name, p.RoleId, x.Key));
                        });
                }).ToList();

                _domain0.Model.UserPermissions.Edit(innerList =>
                {
                    innerList.RemoveMany(upToRemove);
                    innerList.AddRange(upToAdd);
                });
            }
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Apply Roles");
            }
        }

        private Task ResetRoles()
        {
            foreach (var userRole in SelectedUserRoles)
                userRole.Restore();
            IsChangedRoles = false;
            return Task.CompletedTask;
        }

        protected override async Task ApplyPermissions()
        {
            var (toAdd, toRemove) = GetItemsChanges(SelectedItemPermissions, SelectedItemsIds);

            foreach (var rKV in toRemove)
                await _domain0.Client.RemoveUserPermissionsAsync(rKV.Key,
                    new IdArrayRequest(rKV.Value.ToList()));

            var upToRemove = toRemove.SelectMany(x =>
            {
                return _domain0.Model.UserPermissions.Items
                    .Where(up => up.UserId == x.Key && !up.RoleId.HasValue && x.Value.Contains(up.Id.Value));
            }).ToList();

            foreach (var aKV in toAdd)
                await _domain0.Client.AddUserPermissionsAsync(aKV.Key,
                    new IdArrayRequest(aKV.Value.ToList()));

            var upToAdd = toAdd.SelectMany(x =>
            {
                return _domain0.Model.Permissions.Items
                    .Where(p => x.Value.Contains(p.Id.Value))
                    .Select(p => new UserPermission(p.ApplicationId, p.Description, p.Id, p.Name, null, x.Key));
            }).ToList();

            _domain0.Model.UserPermissions.Edit(innerList =>
            {
                innerList.RemoveMany(upToRemove);
                innerList.AddRange(upToAdd);
            });
        }

        // Lock

        private async Task LockUsers(IList list)
        {
            try
            {
                var users = list.Cast<UserProfileViewModel>().ToList();

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
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Lock User");
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
                await _shell.HandleException(e, "Failed to create User");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Task<UserProfile> ForceCreateUserApi()
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
                    return _domain0.Client.ForceCreateUserAsync(requestByPhone);
                case ForceCreateUserModeEnum.Email:
                    var requestByEmail = new ForceCreateEmailUserRequest(
                        BlockEmailSend,
                        CustomEmailSubjectTemplate,
                        CustomEmailTemplate,
                        Email, Name,
                        roles);
                    return _domain0.Client.ForceCreateUser2Async(requestByEmail);
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
