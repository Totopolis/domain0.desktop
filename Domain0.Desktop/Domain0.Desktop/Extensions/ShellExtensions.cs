using Autofac;
using Domain0.Desktop.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public static void ShowException(this IShell shell, Exception ex, string title = "Error")
        {
            var window = shell.GetWindow();
            window.Invoke(() => window.ShowMessageAsync(title, ex.Message));
        }

        internal static async Task<LoadingProgress> ShowProgress(this IShell shell, string title, string message)
        {
            var window = shell.GetWindow();
            var controller = await window.Invoke(() =>
                window.ShowProgressAsync(title, message, false, new MetroDialogSettings {AnimateShow = false}));

            return new LoadingProgress(controller);
        }

        private static MetroWindow GetWindow(this IShell shell)
        {
            return shell.Container.Resolve<IDockWindow>() as MetroWindow;
            //return Application.Current.MainWindow as MetroWindow;
        }
    }

    internal class LoadingProgress
    {
        private readonly ProgressDialogController _ctrl;
        private readonly List<Task> _tasks;

        public LoadingProgress(ProgressDialogController ctrl)
        {
            _ctrl = ctrl;
            _tasks = new List<Task>();
        }

        public LoadingProgress Wait(Task task, string s)
        {
            task = task.ContinueWith(_ => _ctrl.SetMessage($"Loaded - {s}"));
            _tasks.Add(task);
            return this;
        }

        public async Task WaitAll()
        {
            await Task.WhenAll(_tasks);
            await _ctrl.CloseAsync();
            _tasks.Clear();
        }
    }
}
