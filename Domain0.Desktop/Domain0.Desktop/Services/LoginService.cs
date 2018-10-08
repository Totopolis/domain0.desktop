using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using Ui.Wpf.Common;

namespace Domain0.Desktop.Services
{
    public class LoginService : ILoginService
    {
        private readonly IDomain0Service _domain0;
        private readonly IShell _shell;

        public LoginService(IShell shell, IDomain0Service domain0)
        {
            _shell = shell;
            _domain0 = domain0;
        }

        public void ShowLogin(Action onSuccess)
        {
            AuthProcess.Start(
                getAuthenticationData: () =>
                {
                    LoginDialogSettings settings = new LoginDialogSettings
                    {
                        AnimateShow = true,
                        AnimateHide = true,
                        InitialUsername = "",
                        AffirmativeButtonText = "Login",
                        NegativeButtonText = "Exit",
                        NegativeButtonVisibility = Visibility.Visible,
                        UsernameWatermark = "Login",
                        PasswordWatermark = "Password",
                        EnablePasswordPreview = true,
                        RememberCheckBoxVisibility = Visibility.Visible
                    };
                    return (Application.Current.MainWindow as MetroWindow).ShowLoginAsync("Login", "Enter login and password", settings);
                },
                authentication: async (x) =>
                {
                    if (x == null)
                        return false;

                    try
                    {
                        return await _domain0.Login("http://localhost:8880", x.Username, x.Password);
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
}