using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels.Items;
using Domain0.Desktop.Views.Dialogs;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common;
using Environment = Domain0.Api.Client.Environment;

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

            ForceCreateUserRolesFilterCommand = ReactiveCommand
                .Create<string>(x => ForceCreateUserRolesFilter = x)
                .DisposeWith(Disposables);

            LockUsersCommand = ReactiveCommand
                .CreateFromTask<IList>(LockUsers,
                    this.WhenAny(
                        x => x.SelectedItemsIds,
                        x => x.Value != null && x.Value.Count > 0))
                .DisposeWith(Disposables);

            RolesFilterCommand = ReactiveCommand
                .Create<string>(filter => RolesFilter = filter)
                .DisposeWith(Disposables);
            RoleCheckedCommand = ReactiveCommand
                .Create<SelectedUserRoleViewModel>(RoleChecked)
                .DisposeWith(Disposables);

            var rolesChangedObservable = this.WhenAnyValue(x => x.IsChangedRoles);
            ApplyRolesCommand = ReactiveCommand
                .CreateFromTask(ApplyRoles, rolesChangedObservable)
                .DisposeWith(Disposables);
            ResetRolesCommand = ReactiveCommand
                .CreateFromTask(ResetRoles, rolesChangedObservable)
                .DisposeWith(Disposables);

            ForceChangePasswordCommand = ReactiveCommand
                .CreateFromTask(ForceResetPassword, this.WhenHaveOnlyOneSelectedItem())
                .DisposeWith(Disposables);
            ForceChangePhoneCommand = ReactiveCommand
                .CreateFromTask(ForceChangePhone)
                .DisposeWith(Disposables);
            ForceChangeEmailCommand = ReactiveCommand
                .CreateFromTask(ForceChangeEmail)
                .DisposeWith(Disposables);
            
            // Permissions

            SubscribeToPermissions(
                    _domain0.Model.UserPermissions,
                    (permissionId, items) => items
                        .Where(x => !x.RoleId.HasValue && x.Id.Value == permissionId)
                        .Select(x => x.UserId))
                .DisposeWith(Disposables);
                
            // Roles

            var dynamicForceCreateUserRolesFilter = this
                .WhenAnyValue(x => x.ForceCreateUserRolesFilter)
                .Select(Filters.CreateRolesPredicate);

            _domain0.Model.Roles.Connect()
                .Filter(dynamicForceCreateUserRolesFilter)
                .Transform(x => new ForceCreateUserRoleViewModel { Role = x })
                .Sort(SortExpressionComparer<ForceCreateUserRoleViewModel>.Ascending(x => x.Role.Id))
                .ObserveOnDispatcher()
                .Bind(out _forceCreateUserRoles)
                .Subscribe()
                .DisposeWith(Disposables);

            var dynamicRolesFilter = this
                .WhenAnyValue(x => x.RolesFilter)
                .Select(Filters.CreateRolesPredicate);

            var sourceUserRoles = _domain0.Model.UserRoles
                .Connect()
                .ToCollection();
            var sourceRoles = _domain0.Model.Roles
                .Connect()
                .Filter(dynamicRolesFilter)
                .ToCollection();
            var sourceSelected = this
                .WhenAnyValue(x => x.SelectedItemsIds);

            Observable.CombineLatest(
                    sourceUserRoles,
                    sourceRoles,
                    sourceSelected,
                    (userRoles, roles, selectedIds) => selectedIds.Count > 0
                        ? new {userRoles, roles, selectedIds}
                        : null)
                .Throttle(TimeSpan.FromSeconds(.1))
                .Synchronize()
                .Select(o => o?.roles
                    .Select(r =>
                    {
                        var roleId = r.Id.Value;
                        var userIds = o.userRoles
                            .Where(x => x.Id.Value == roleId)
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
                    SelectedUserRoles = x;
                    IsChangedRoles = false;
                })
                .DisposeWith(Disposables);

            // Environments

            _domain0.Model.Environments.Connect()
                .Sort(SortExpressionComparer<Environment>.Ascending(x => x.Id))
                .ObserveOnDispatcher()
                .Bind(out _environments)
                .Subscribe()
                .DisposeWith(Disposables);

            // Locales

            var dynamicLocaleFilter = this
                .WhenAnyValue(x => x.Environment)
                .Select(Filters.CreateEnvironmentPredicate);

            _domain0.Model.MessageTemplates.Connect()
                .Filter(dynamicLocaleFilter)
                .Group(x => x.Locale)
                .Transform(x => x.Key)
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .ObserveOnDispatcher()
                .Bind(out _locales)
                .Subscribe()
                .DisposeWith(Disposables);
        }

        private ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> _forceCreateUserRoles;
        public ReadOnlyObservableCollection<ForceCreateUserRoleViewModel> ForceCreateUserRoles => _forceCreateUserRoles;

        [Reactive] public IEnumerable<SelectedUserRoleViewModel> SelectedUserRoles {get; set; }
        [Reactive] public string RolesFilter { get; set; }
        public ReactiveCommand<string, Unit> RolesFilterCommand { get; set; }
        public ReactiveCommand<SelectedUserRoleViewModel, Unit> RoleCheckedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ApplyRolesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ResetRolesCommand { get; set; }
        [Reactive] public bool IsChangedRoles { get; set; }

        public ReactiveCommand<Unit, Unit> ForceChangePasswordCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ForceChangePhoneCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ForceChangeEmailCommand { get; set; }

        public ReactiveCommand<IList, Unit> LockUsersCommand { get; set; }

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

                var urToRemove = toRemove.SelectMany(x =>
                {
                    return _domain0.Model.UserRoles.Items
                        .Where(r => r.UserId == x.Key && x.Value.Contains(r.Id.Value));
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

                var urToAdd = toAdd.SelectMany(x =>
                {
                    return x.Value.Select(v =>
                    {
                        var r = _domain0.Model.Roles.Lookup(v).Value;
                        return new UserRole(r.Description, r.Id, r.IsDefault, r.Name, x.Key);
                    });
                }).ToList();

                _domain0.Model.UserPermissions.Edit(innerList =>
                {
                    innerList.RemoveMany(upToRemove);
                    innerList.AddRange(upToAdd);
                });

                _domain0.Model.UserRoles.Edit(innerList =>
                {
                    innerList.RemoveMany(urToRemove);
                    innerList.AddRange(urToAdd);
                });

                TraceApplied("Roles to Users", toAdd, toRemove);
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

            TraceApplied("Permissions to Users", toAdd, toRemove);
        }

        // Reset Password

        private async Task ForceResetPassword()
        {
            try
            {
                var dialogResult = await _shell.ShowForceResetPasswordDialog(Locales.FirstOrDefault(), Locales.ToList());
                if (dialogResult == null)
                    return;

                var userId = dialogResult.ResetWay == ResetWayType.UserId ? EditViewModel.Id : null;
                var email = dialogResult.ResetWay == ResetWayType.Email ? EditViewModel.Email : null;
                var phone = dialogResult.ResetWay == ResetWayType.Phone
                    ? (long.TryParse(EditViewModel.Phone, out var phoneParsed)
                        ? phoneParsed
                        : (long?)null)
                    : null;

                var request = new ForceResetPasswordRequest(
                    email,
                    dialogResult.Locale,
                    phone,
                    userId
                );
                await _domain0.Client.ForceResetUserPasswordAsync(request);
            }
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Force Reset Password");
            }
        }

        // Change Email & Phone

        private async Task ForceChangePhone()
        {
            var changeData = await _shell.ShowForceChangeDialog(
                EditViewModel.Phone,
                "Phone",
                Locales.FirstOrDefault(),
                Locales.ToList());

            if (changeData != null)
            {
                try
                {
                    var newPhone = long.Parse(changeData.Input);
                    var request = new ChangePhoneRequest(changeData.Locale, newPhone, EditViewModel.Id.Value);
                    await _domain0.Client.ForceChangePhoneAsync(request);
                    EditViewModel.Phone = changeData.Input;
                    Models.AddOrUpdate(EditModel);
                }
                catch (Exception e)
                {
                    await _shell.HandleException(e, "Failed to Change Phone");
                }
            }
        }

        private async Task ForceChangeEmail()
        {
            var changeData = await _shell.ShowForceChangeDialog(
                EditViewModel.Email,
                "Email",
                Locales.FirstOrDefault(),
                Locales.ToList());

            if (changeData != null)
            {
                try
                {
                    var request = new ChangeEmailRequest(changeData.Locale, changeData.Input, EditViewModel.Id.Value);
                    await _domain0.Client.ForceChangeEmailAsync(request);
                    EditViewModel.Email = changeData.Input;
                    Models.AddOrUpdate(EditModel);
                }
                catch (Exception e)
                {
                    await _shell.HandleException(e, "Failed to Change Email");
                }
            }
        }

        // Lock

        private async Task LockUsers(IList list)
        {
            try
            {
                var users = list.Cast<UserProfileViewModel>().ToList();

                var toLock = users.Any(x => !x.IsLocked);

                Trace.TraceInformation("{0} users: {1}", toLock ? "Lock" : "Unlock", string.Join(", ", users.Where(x => x.IsLocked != toLock).Select(x => x.Id.Value)));

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

        private ReadOnlyObservableCollection<string> _locales;
        public ReadOnlyObservableCollection<string> Locales => _locales;
        [Reactive] public string Locale { get; set; }
        private ReadOnlyObservableCollection<Environment> _environments;
        public ReadOnlyObservableCollection<Environment> Environments => _environments;
        [Reactive] public Environment Environment { get; set; }

        [Reactive] public string ForceCreateUserRolesFilter { get; set; }
        public ReactiveCommand<string, Unit> ForceCreateUserRolesFilterCommand { get; set; }
        
        public int ForceCreateUserMode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string CustomSmsTemplate { get; set; }
        public string CustomEmailSubjectTemplate { get; set; }
        public string CustomEmailTemplate { get; set; }
        public bool BlockSmsSend { get; set; }
        public bool BlockEmailSend { get; set; }

        protected override async Task Create()
        {
            var createdItemSubscription = Disposable.Empty;
            try
            {
                var roles = ForceCreateUserRoles
                    .Where(x => x.IsSelected)
                    .Select(x => x.Role)
                    .ToList();

                var userProfile = await ForceCreateUserApi(roles);

                createdItemSubscription = HandleInteractionOnCreatedItemInList(userProfile.Id);
                
                var rolesIds = roles
                    .Select(x => x.Id.Value)
                    .ToList();
                var userPermissions =
                    from rp in _domain0.Model.RolePermissions.Items
                    where rolesIds.Contains(rp.RoleId)
                    join p in _domain0.Model.Permissions.Items on rp.Id equals p.Id.Value
                    select new UserPermission(p.ApplicationId, p.Description, p.Id, p.Name, rp.RoleId, userProfile.Id);
                _domain0.Model.UserPermissions.AddRange(userPermissions);
                Models.AddOrUpdate(userProfile);

                Trace.TraceInformation("Created {0}: {1} with roles [{2}]", typeof(UserProfile).Name, userProfile.Id, string.Join(", ", roles.Select(x => x.Name)));

                IsCreateFlyoutOpen = false;
            }
            catch (Exception e)
            {
                createdItemSubscription.Dispose();
                await _shell.HandleException(e, "Failed to create User");
            }
        }

        private Task<UserProfile> ForceCreateUserApi(IEnumerable<Role> roles)
        {
            var rolesNames = roles
                .Select(x => x.Name)
                .ToList();

            switch ((ForceCreateUserModeEnum)ForceCreateUserMode)
            {
                case ForceCreateUserModeEnum.Phone:
                    var phone = long.Parse(Phone);
                    var requestByPhone = new ForceCreateUserRequest(
                        BlockSmsSend,
                        CustomSmsTemplate,
                        Environment?.Token,
                        Locale,
                        Name,
                        phone,
                        rolesNames);
                    return _domain0.Client.ForceCreateUserAsync(requestByPhone);
                case ForceCreateUserModeEnum.Email:
                    var requestByEmail = new ForceCreateEmailUserRequest(
                        BlockEmailSend,
                        CustomEmailSubjectTemplate,
                        CustomEmailTemplate,
                        Email,
                        Environment?.Token,
                        Locale,
                        Name,
                        rolesNames);
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
