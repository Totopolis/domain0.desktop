using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain0.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace Domain0.Desktop.Extensions
{
    public static class ShellExtensions
    {

        public static void ShowUsers(this IShell shell)
        {
            shell.ShowView<ManageUsersView>(new ViewRequest("manage-users"), new UiShowOptions { Title = "Users" });
        }

        public static void ShowRoles(this IShell shell)
        {
            shell.ShowView<ManageRolesView>(new ViewRequest("manage-roles"), new UiShowOptions { Title = "Roles" });
        }

        public static void ShowPermissions(this IShell shell)
        {
            shell.ShowView<ManagePermissionsView>(new ViewRequest("manage-permissions"), new UiShowOptions { Title = "Permissions" });
        }

        public static void ShowApplications(this IShell shell)
        {
            shell.ShowView<ManageApplicationsView>(new ViewRequest("manage-applications"), new UiShowOptions { Title = "Applications" });
        }

        public static void ShowMessages(this IShell shell)
        {
            shell.ShowView<ManageMessagesView>(new ViewRequest("manage-messages"), new UiShowOptions { Title = "Messages" });
        }

    }
}
