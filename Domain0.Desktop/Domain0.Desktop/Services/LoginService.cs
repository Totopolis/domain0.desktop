using Domain0.Desktop.Properties;
using Domain0.Desktop.Views.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using Ui.Wpf.Common;

namespace Domain0.Desktop.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDomain0Service _domain0;
        
        public LoginService(IDomain0Service domain0)
        {
            _domain0 = domain0;

            _domain0.HostUrl = Settings.Default.HostUrl;
        }

        public bool LoadPreviousToken()
        {
            return _domain0.LoadToken();
        }

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
                        InitialUsername = "",
                        AffirmativeButtonText = "Login",
                        NegativeButtonText = "Exit",
                        NegativeButtonVisibility = Visibility.Visible,
                        UrlWatermark = "Url",
                        ShouldHideUrl = !canChangeUrl,
                        InitialUrl = _domain0.HostUrl,
                        UsernameWatermark = "Login",
                        PasswordWatermark = "Password",
                        EnablePasswordPreview = true,
                        RememberCheckBoxVisibility = Visibility.Visible
                    };

                    var loginDialog = new LoginWithUrlDialog(view, settings)
                    {
                        Title = "Login",
                        Message = "Enter login and password"
                    };

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
                        _domain0.HostUrl = x.Url;

                        var token = await _domain0.Login(x.Username, x.Password);
                        if (token != null)
                        {
                            _domain0.UpdateAccessToken(token, x.ShouldRemember);
                            return true;
                        }
                        else
                            return false;
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