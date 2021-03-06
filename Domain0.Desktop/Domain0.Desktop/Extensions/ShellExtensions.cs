﻿using Autofac;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monik.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain0.Desktop.Views.Dialogs;
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

        public static void ShowEnvironments(this IShell shell)
        {
            shell.ShowView<ManageEnvironmentsView>(new ViewRequest("manage-environments"), new UiShowOptions { Title = "Environments" });
        }

        public static void ShowMessages(this IShell shell)
        {
            shell.ShowView<ManageMessagesView>(new ViewRequest("manage-messages"), new UiShowOptions { Title = "Messages" });
        }

        public static async Task HandleException(this IShell shell, Exception ex, string title = "Error",
            bool log = true, bool reconnect = false)
        {
            if (log)
            {
                shell.Container.Resolve<IMonik>()
                    .ApplicationWarning($"{title}: {ex}");
            }

            // show error to user
            var window = shell.GetWindow();
            await window.Invoke(() => window.ShowMessageAsync(title, ex.Message));

            if (reconnect ||
                ex is AuthenticationContextException ||
                ex is Domain0ClientException clientException
                && clientException.StatusCode == 401)
            {
                shell.Container.Resolve<IDomain0Service>()
                    .Reconnect();
            }
        }

        internal static Task<string> ShowInput(this IShell shell, string title, string message, string defaultText)
        {
            var window = shell.GetWindow();
            return window.Invoke(() =>
                window.ShowInputAsync(title, message,
                    new MetroDialogSettings
                    {
                        DefaultText = defaultText
                    }));
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

        internal static Task<ForceResetPasswordDialogData> ShowForceResetPasswordDialog(this IShell shell,
            string locale, List<string> locales)
        {
            var window = shell.GetWindow();
            return window.Invoke(async () =>
            {
                var forceResetPasswordDialog = new ForceResetPasswordDialog(window, new ForceResetPasswordDialogSettings
                    {
                        LocaleInitial = locale,
                        Locales = locales
                    })
                    {Title = "Force Reset Password"};
                await window.ShowMetroDialogAsync(forceResetPasswordDialog);
                var result = await forceResetPasswordDialog.WaitForButtonPressAsync();
                await window.HideMetroDialogAsync(forceResetPasswordDialog);
                return result;
            });
        }

        internal static Task<ForceChangeDialogData> ShowForceChangeDialog(this IShell shell,
            string input, string inputLabel, string locale, List<string> locales)
        {
            var window = shell.GetWindow();
            return window.Invoke(async () =>
            {
                var forceChangeDialog = new ForceChangeDialog(window, new ForceChangeDialogSettings
                    {
                        InputInitial = input,
                        InputLabel = inputLabel,
                        LocaleInitial = locale,
                        Locales = locales
                    })
                    {Title = $"Force Change {inputLabel}"};
                await window.ShowMetroDialogAsync(forceChangeDialog);
                var result = await forceChangeDialog.WaitForButtonPressAsync();
                await window.HideMetroDialogAsync(forceChangeDialog);
                return result;
            });
        }

        internal static Task<ChangePasswordDialogData> ShowChangePasswordDialog(this IShell shell)
        {
            var window = shell.GetWindow();
            return window.Invoke(async () =>
            {
                var changePasswordDialog = new ChangePasswordDialog(window, new ChangePasswordDialogSettings())
                    {Title = "Change My Password"};
                await window.ShowMetroDialogAsync(changePasswordDialog);
                var result = await changePasswordDialog.WaitForButtonPressAsync();
                await window.HideMetroDialogAsync(changePasswordDialog);
                return result;
            });
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
