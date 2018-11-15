using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Models;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common;

namespace Domain0.Desktop.Services
{
    public class Domain0Service : IDomain0Service
    {
        private readonly IShell _shell;
        private readonly ILoginService _login;
        private readonly IDomain0AuthenticationContext _authContext;

        public Domain0Service(IShell shell, ILoginService login, IDomain0AuthenticationContext domain0AuthenticationContext)
        {
            _shell = shell;
            _login = login;
            _authContext = domain0AuthenticationContext;

            Model = new Domain0Model();
        }

        public IDomain0Client Client => _authContext.Client;

        public Domain0Model Model { get; }

        public void Reconnect()
        {
            _login.Logout(async () => await LoadModel());
        }

        public async Task LoadModel()
        {
            try
            {
                await LoadModelInternal();
            }
            catch (Exception ex)
            {
                await _shell.HandleException(ex, "Failed to Load model", reconnect: true);
            }
        }

        private async Task LoadModelInternal()
        {
            var userProfilesTask = _authContext.Client
                .GetAllUsersAsync();
            var initUserProfilesTask = CreateTaskInitCache(userProfilesTask, Model.UserProfiles);

            var rolesTask = _authContext.Client
                .LoadRolesByFilterAsync(new RoleFilter(new List<int>()));
            var initRolesTask = CreateTaskInitCache(rolesTask, Model.Roles);

            var permissionsTask = _authContext.Client
                .LoadPermissionsByFilterAsync(new PermissionFilter(new List<int>()));
            var initPermissionsTask = CreateTaskInitCache(permissionsTask, Model.Permissions);

            var applicationsTask = _authContext.Client
                .LoadApplicationsByFilterAsync(new ApplicationFilter(new List<int>()));
            var initApplicationsTask = CreateTaskInitCache(applicationsTask, Model.Applications);

            var environmentsTask = _authContext.Client
                .LoadEnvironmentsByFilterAsync(new EnvironmentFilter(null, true));
            var initEnvironmentsTask = CreateTaskInitCache(environmentsTask, Model.Environments);

            var messageTemplatesTask = _authContext.Client
                .LoadMessageTemplatesByFilterAsync(new MessageTemplateFilter(new List<int>()));
            var initMessageTemplatesTask = CreateTaskInitCache(messageTemplatesTask, Model.MessageTemplates);

            var userRolesTask = _authContext.Client
                .LoadRolesByUserFilterAsync(new RoleUserFilter(new List<int>()));
            var initUserRolesTask = CreateTaskInitList(userRolesTask, Model.UserRoles);

            var userPermissionsTask = userProfilesTask
                .ContinueWith(task =>
                {
                    var userIds = task.Result.Select(x => x.Id).ToList();
                    var loadTask = _authContext.Client
                        .LoadPermissionsByUserFilterAsync(new UserPermissionFilter(userIds));
                    var initTask = CreateTaskInitList(loadTask, Model.UserPermissions);
                    return Task.WhenAll(loadTask, initTask);
                }, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();

            var rolePermissionsTask = rolesTask
                .ContinueWith(task =>
                {
                    var roleIds = task.Result.Select(x => x.Id.Value).ToList();
                    var loadTask = _authContext.Client
                        .LoadPermissionsByRoleFilterAsync(new RolePermissionFilter(roleIds));
                    var initTask = CreateTaskInitList(loadTask, Model.RolePermissions);
                    return Task.WhenAll(loadTask, initTask);
                }, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();

            await _shell.ShowProgress("Loading data...", "Load everything", false)
                .Wait(userProfilesTask, "Users - Loaded")
                .Wait(initUserProfilesTask, "Users - Initialized")

                .Wait(rolesTask, "Roles - Loaded")
                .Wait(initRolesTask, "Roles - Initialized")

                .Wait(permissionsTask, "Permissions - Loaded")
                .Wait(initPermissionsTask, "Permissions - Initialized")

                .Wait(applicationsTask, "Applications - Loaded")
                .Wait(initApplicationsTask, "Applications - Initialized")

                .Wait(environmentsTask, "Environments - Loaded")
                .Wait(initEnvironmentsTask, "Environments - Initialized")

                .Wait(messageTemplatesTask, "Message Templates - Loaded")
                .Wait(initMessageTemplatesTask, "Message Templates - Initialized")

                .Wait(userRolesTask, "User's Roles - Loaded")
                .Wait(initUserRolesTask, "User's Roles - Initialized")

                .Wait(userPermissionsTask, "User's Permissions - Loaded")
                .Wait(rolePermissionsTask, "Role's Permissions - Loaded")
                .WaitAll();
        }


        private static Task CreateTaskInitCache<TValue, TKey>(Task<List<TValue>> loadTask, ISourceCache<TValue, TKey> cache)
        {
            return loadTask
                .ContinueWith(task =>
                {
                    cache.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private static Task CreateTaskInitList<TValue>(Task<List<TValue>> loadTask, ISourceList<TValue> cache)
        {
            return loadTask
                .ContinueWith(task =>
                {
                    cache.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddRange(task.Result);
                    });
                    return task.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

    }
}