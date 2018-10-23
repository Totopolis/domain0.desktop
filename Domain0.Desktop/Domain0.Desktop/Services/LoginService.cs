using Domain0.Desktop.Properties;
using Domain0.Desktop.Views.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using Domain0.Api.Client;
using Ui.Wpf.Common;
using Application = System.Windows.Application;

namespace Domain0.Desktop.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDomain0AuthenticationContext _domain0Context;

        public LoginService(
            IDomain0AuthenticationContext domain0AuthenticationContext)
        {
            _domain0Context = domain0AuthenticationContext;
            _domain0Context.HostUrl = Settings.Default.HostUrl;
        }

        public bool IsLoggedIn => _domain0Context.IsLoggedIn;
       

        public void ShowLogin(bool canChangeUrl, Action onSuccess = null)
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


                        switch (x.LoginMode)
                        {
                            case LoginMode.Phone:
                                return await _domain0Context.LoginByPhone(long.Parse(x.Phone), x.Password);

                            case LoginMode.Email:
                                return await _domain0Context.LoginByEmail(x.Email, x.Password);

                            default:
                                throw new ArgumentException("Unknown login mode");
                        }
                    }
                    catch (Exception e)
                    {
                        await (Application.Current.MainWindow as MainWindow).ShowMessageAsync("Login failed", $"Failed to login\n({e.Message})");
                        return false;
                    }
                },
                authenticationSuccess: () => onSuccess?.Invoke(),
                authenticationFail: () => Application.Current.MainWindow?.Close()
            );
        }
    }


    internal class AuthProcess
    {
        private const int LoginTryCount = 3;

        public static async void Start<T>(
            Func<Task<T>> getAuthenticationData,
            Func<T, Task<bool>> authentication,
            Action authenticationSuccess,
            Action authenticationFail)
        {
            var trysLeft = LoginTryCount;
            while (trysLeft-- > 0)
            {
                var loginData = await getAuthenticationData().ConfigureAwait(true);
                if (loginData == null)
                {
                    authenticationFail();
                    return;
                }

                var authentcationResult = await authentication(loginData).ConfigureAwait(true);
                if (authentcationResult)
                {
                    authenticationSuccess();
                    return;
                }
            }

            authenticationFail();
        }
    }
}