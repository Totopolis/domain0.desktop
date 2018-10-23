using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ui.Wpf.Common;

namespace Domain0.Desktop.Services
{
    public class Domain0Service : IDomain0Service
    {
        private readonly IShell _shell;
        private readonly IDomain0AuthenticationContext _authContext;

        public Domain0Service(IShell shell, IDomain0AuthenticationContext domain0AuthenticationContext)
        {
            _shell = shell;
            _authContext = domain0AuthenticationContext;

            Model = new Domain0Model();
        }

        public IDomain0Client Client => _authContext.Client;

        public Domain0Model Model { get; }

        public async Task LoadModel()
        {
            var userProfilesTask = _authContext.Client
                .GetUserByFilterAsync(new UserProfileFilter(new List<int>()))
                .ContinueWith(task =>
                {
                    Model.UserProfiles.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                });

            var rolesTask = _authContext.Client
                .LoadRolesByFilterAsync(new RoleFilter(new List<int>()))
                .ContinueWith(task =>
                {
                    Model.Roles.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                });

            var permissionsTask = _authContext.Client
                .LoadPermissionsByFilterAsync(new PermissionFilter(new List<int>()))
                .ContinueWith(task =>
                {
                    Model.Permissions.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                });

            var applicationsTask = _authContext.Client
                .LoadApplicationsByFilterAsync(new ApplicationFilter(new List<int>()))
                .ContinueWith(task =>
                {
                    Model.Applications.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                });

            var messageTemplatesTask = _authContext.Client
                .LoadMessageTemplatesByFilterAsync(new MessageTemplateFilter(new List<int>()))
                .ContinueWith(task =>
                {
                    Model.MessageTemplates.Edit(innerCache =>
                    {
                        innerCache.Clear();
                        innerCache.AddOrUpdate(task.Result);
                    });
                    return task.Result;
                });

            var userPermissionsTask = userProfilesTask
                .ContinueWith(task =>
                {
                    var userIds = task.Result.Select(x => x.Id).ToList();
                    return _authContext.Client
                        .LoadPermissionsByUserFilterAsync(new UserPermissionFilter(userIds))
                        .ContinueWith(innerTask =>
                        {
                            Model.UserPermissions.Edit(innerList =>
                            {
                                innerList.Clear();
                                innerList.AddRange(innerTask.Result);
                            });
                            return innerTask.Result;
                        });
                });

            var rolePermissionsTask = rolesTask
                .ContinueWith(task =>
                {
                    var roleIds = task.Result.Select(x => x.Id.Value).ToList();
                    return _authContext.Client
                        .LoadPermissionsByRoleFilterAsync(new RolePermissionFilter(roleIds))
                        .ContinueWith(innerTask =>
                        {
                            Model.RolePermissions.Edit(innerList =>
                            {
                                innerList.Clear();
                                innerList.AddRange(innerTask.Result);
                            });
                            return innerTask.Result;
                        });
                });

            var progress = await _shell.ShowProgress("Loading data...", "Load everything");
            await progress
                .Wait(userProfilesTask, "Users")
                .Wait(rolesTask, "Roles")
                .Wait(permissionsTask, "Permissions")
                .Wait(applicationsTask, "Applications")
                .Wait(messageTemplatesTask, "Message Templates")
                .Wait(userPermissionsTask, "User's Permissions")
                .Wait(rolePermissionsTask, "Role's Permissions")
                .WaitAll();
        }

    }
}