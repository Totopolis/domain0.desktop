using Autofac;
using Domain0.Desktop.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monik.Common;
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

        public static Task HandleException(this IShell shell, Exception ex, string title = "Error")
        {
            var monik = shell.Container.Resolve<IMonik>();
            monik.ApplicationWarning($"{title}: {ex}");

            var window = shell.GetWindow();
            return window.Invoke(() => window.ShowMessageAsync(title, ex.Message));
        }

        internal static LoadingProgress ShowProgress(this IShell shell, string title, string message,
            bool animatedShow = true, bool animatedHide = true)
        {
            var window = shell.GetWindow();
            var controllerTask = window.Invoke(() =>
                window.ShowProgressAsync(title, message, false,
                    new MetroDialogSettings
                    {
                        AnimateShow = animatedShow,
                        AnimateHide = animatedHide
                    }));

            return new LoadingProgress(controllerTask);
        }

        private static MetroWindow GetWindow(this IShell shell)
        {
            return shell.Container.Resolve<IDockWindow>() as MetroWindow;
            //return Application.Current.MainWindow as MetroWindow;
        }
    }

    internal class LoadingProgress
    {
        private readonly Task<ProgressDialogController> _ctrl;
        private readonly List<Task> _tasks;

        public LoadingProgress(Task<ProgressDialogController> ctrl)
        {
            _ctrl = ctrl;
            _ctrl.ContinueWith(t =>
            {
                if (t.Result.IsOpen)
                    t.Result.SetIndeterminate();
            });
            _tasks = new List<Task>();
        }

        public LoadingProgress Wait(Task task, string s = null)
        {
            _tasks.Add(task);

            if (!string.IsNullOrEmpty(s))
            {
                task.ContinueWith(_ =>
                {
                    if (_ctrl.IsCompleted && _ctrl.Result.IsOpen)
                        _ctrl.Result.SetMessage(s);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            return this;
        }

        public async Task WaitOnly(Task task)
        {
            try
            {
                await task;
            }
            finally
            {
                await Close();
            }
        }

        public async Task<TResult> WaitOnly<TResult>(Task<TResult> task)
        {
            try
            {
                return await task;
            }
            finally
            {
                await Close();
            }
        }

        public async Task WaitAll()
        {
            try
            {
                await Task.WhenAll(_tasks);
            }
            finally
            {
                _tasks.Clear();
                await Close();
            }
        }

        private async Task Close()
        {
            await _ctrl.ContinueWith(t => t.Result.CloseAsync()).Unwrap();
        }
    }
}
