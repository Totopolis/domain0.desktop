using Autofac;
using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Views.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using Domain0.Desktop.Config;
using Ui.Wpf.Common;
using Application = System.Windows.Application;

namespace Domain0.Desktop.Services
{
    public class LoginService : ILoginService
    {
        private readonly IShell _shell;
        private readonly IAuthenticationContext _domain0Context;
        private readonly IAppConfigStorage _appConfigStorage;

        public LoginService(
            IShell shell,
            IAuthenticationContext domain0AuthenticationContext,
            IAppConfigStorage appConfigStorage)
        {
            _shell = shell;
            _domain0Context = domain0AuthenticationContext;
            _appConfigStorage = appConfigStorage;
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
            var config = _appConfigStorage.Load();
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
                        InitialUrl = config.HostUrl,
                        EmailWatermark = "email@gmail.com",
                        PhoneWatermark = "79998887766",
                        PasswordWatermark = "Password",
                        EnablePasswordPreview = true,
                        RememberCheckBoxVisibility = Visibility.Visible,
                        RememberCheckBoxChecked = config.ShouldRemember,
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
                        config.HostUrl = x.Url;
                        config.ShouldRemember = x.ShouldRemember;
                        _appConfigStorage.Save(config);
                        
                        _domain0Context.HostUrl = x.Url;
                        _domain0Context.ShouldRemember = x.ShouldRemember;

                        var loginTask = GetLoginTask(x);

                        var userProfile = await _shell.ShowProgress("Login", "Trying to login...")
                            .WaitOnly(loginTask);

                        var login = x.LoginMode == LoginMode.Email
                            ? userProfile.Email
                            : userProfile.Phone;
                        _shell.Container.Resolve<IDockWindow>().Title =
                            $"Domain0.Desktop - {userProfile.Name} - {login}";

                        return true;
                    }
                    catch (AuthenticationContextException e)
                    {
                        await _shell.HandleException(e.InnerException, "Login failed", false);
                        return false;
                    }
                },
                authenticationSuccess: () => onSuccess?.Invoke(),
                authenticationFail: () => Application.Current.MainWindow?.Close()
            );
        }

        private Task<UserProfile> GetLoginTask(LoginWithUrlDialogData x)
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