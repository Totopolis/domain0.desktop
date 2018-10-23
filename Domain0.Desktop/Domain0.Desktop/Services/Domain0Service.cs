using Domain0.Api.Client;
using Domain0.Desktop.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain0.Desktop.Services
{
    public class Domain0Service : IDomain0Service
    {
        private readonly IDomain0AuthenticationContext authContext;

        public Domain0Service(IDomain0AuthenticationContext domain0AuthenticationContext)
        {
            authContext = domain0AuthenticationContext;

            Model = new Domain0Model();
        }

        public IDomain0Client Client => authContext.Client;

        public Domain0Model Model { get; }

        public async Task LoadModel()
        {
            var controller = await (System.Windows.Application.Current.MainWindow as MetroWindow)
                .ShowProgressAsync("Loading data...", "Load everything");


            controller.SetMessage("Load User Profiles...");
            var userProfiles = await authContext.Client.GetUserByFilterAsync(new UserProfileFilter(new List<int>()));
            Model.UserProfiles.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(userProfiles);
            });

            controller.SetMessage("Load Permissions...");
            var permissions = await authContext.Client.LoadPermissionsByFilterAsync(new PermissionFilter(new List<int>()));
            Model.Permissions.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(permissions);
            });

            controller.SetMessage("Load Roles...");
            var roles = await authContext.Client.LoadRolesByFilterAsync(new RoleFilter(new List<int>()));
            Model.Roles.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(roles);
            });

            controller.SetMessage("Load Appliations...");
            var applications = await authContext.Client.LoadApplicationsByFilterAsync(new ApplicationFilter(new List<int>()));
            Model.Applications.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(applications);
            });


            controller.SetMessage("Load MessageTemplates...");
            var messageTemplates = await authContext.Client.LoadMessageTemplatesByFilterAsync(new MessageTemplateFilter(new List<int>()));
            Model.MessageTemplates.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(messageTemplates);
            });

            controller.SetMessage("Load Role's Permissions...");
            var roleIds = roles.Select(x => x.Id.Value).ToList();
            var rolePermissions = await authContext.Client.LoadPermissionsByRoleFilterAsync(new RolePermissionFilter(roleIds));
            Model.RolePermissions.Edit(innerList =>
            {
                innerList.Clear();
                innerList.AddRange(rolePermissions);
            });

            controller.SetMessage("Load User's Permissions...");
            var userIds = userProfiles.Select(x => x.Id).ToList();
            var userPermissions = await authContext.Client.LoadPermissionsByUserFilterAsync(new UserPermissionFilter(userIds));
            Model.UserPermissions.Edit(innerList =>
            {
                innerList.Clear();
                innerList.AddRange(userPermissions);
            });

            await controller.CloseAsync();
        }

    }
}