using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Properties;
using Domain0.Desktop.Views.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using Ui.Wpf.Common;
using Application = System.Windows.Application;

namespace Domain0.Desktop.Services
{
    public class LoginService : ILoginService
    {
        private readonly IShell _shell;
        private readonly IDomain0AuthenticationContext _domain0Context;

        public LoginService(
            IShell shell,
            IDomain0AuthenticationContext domain0AuthenticationContext)
        {
            _shell = shell;
            _domain0Context = domain0AuthenticationContext;
            _domain0Context.HostUrl = Settings.Default.HostUrl;
        }

        public void ShowLogin(Action onSuccess = null)
        {
            ShowLoginInternal(true, onSuccess);
        }

        public void Logout(Action onRelogin = null)
        {
            _domain0Context.Logout();
            ShowLoginInternal(true, onRelogin);
        }

        public bool IsLoggedIn => _domain0Context.IsLoggedIn;

        private void ShowLoginInternal(bool canChangeUrl, Action onSuccess = null)
        {
            AuthProcess.Start(
                getAuthenticationData: async () =>
                {
                    var view = (Application.Current.MainWindow as MetroWindow);

                    LoginWithUrlDialogSettings settings = new LoginWithUrlDialogSettings
                    {
                        AnimateShow = true,
                        AnimateHide = true,
                        AffirmativeButtonText = "Login",
                        NegativeButtonText = "Exit",
                        NegativeButtonVisibility = Visibility.Visible,
                        UrlWatermark = "Url",
                        ShouldHideUrl = !canChangeUrl,
                        InitialUrl = _domain0Context.HostUrl,
                        EmailWatermark = "email@gmail.com",
                        PhoneWatermark = "79998887766",
                        PasswordWatermark = "Password",
                        EnablePasswordPreview = true,
                        RememberCheckBoxVisibility = Visibility.Visible
                    };

                    var loginDialog = new LoginWithUrlDialog(view, settings) { Title = "Login" };
                    await view.ShowMetroDialogAsync(loginDialog);
                    var result = await loginDialog.WaitForButtonPressAsync();
                    await view.HideMetroDialogAsync(loginDialog);
                    return result;
                },
                authentication: async (x) =>
                {
                    if (x == null)
                        return false;

                    try
                    {
                        Settings.Default.HostUrl = x.Url;
                        Settings.Default.Save();
                        _domain0Context.HostUrl = x.Url;
                        _domain0Context.ShouldRemember = x.ShouldRemember;

                        var loginTask = GetLoginTask(x);

                        return await _shell.ShowProgress("Login", "Trying to login...")
                            .WaitOnly(loginTask);
                    }
                    catch (Exception e)
                    {
                        await _shell.HandleException(e, "Login failed");
                        return false;
                    }
                },
                authenticationSuccess: () => onSuccess?.Invoke(),
                authenticationFail: () => Application.Current.MainWindow?.Close()
            );
        }

        private Task<bool> GetLoginTask(LoginWithUrlDialogData x)
        {
            switch (x.LoginMode)
            {
                case LoginMode.Phone:
                    return _domain0Context.LoginByPhone(long.Parse(x.Phone), x.Password);

                case LoginMode.Email:
                    return _domain0Context.LoginByEmail(x.Email, x.Password);

                default:
                    throw new ArgumentException("Unknown login mode");
            }
        }
    }
}