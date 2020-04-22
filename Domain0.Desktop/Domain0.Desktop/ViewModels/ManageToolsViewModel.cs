using Domain0.Api.Client;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using MahApps.Metro;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Domain0.Desktop.Config;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;
using Application = System.Windows.Application;

namespace Domain0.Desktop.ViewModels
{
    public class ManageToolsViewModel : ViewModelBase
    {
        private readonly IShell _shell;
        private readonly IDomain0Service _domain0;
        private readonly IAuthenticationContext _authContext;
        private readonly IAppConfigStorage _appConfigStorage;

        public ManageToolsViewModel(
            IShell shell,
            IDomain0Service domain0,
            IAuthenticationContext authContext,
            IAppConfigStorage appConfigStorage)
        {
            _shell = shell;
            _domain0 = domain0;
            _authContext = authContext;
            _appConfigStorage = appConfigStorage;

            LogoutCommand = ReactiveCommand
                .Create(Logout)
                .DisposeWith(Disposables);
            ReloadCommand = ReactiveCommand
                .CreateFromTask(Reload)
                .DisposeWith(Disposables);
            CopyTokenToClipboardCommand = ReactiveCommand
                .Create(CopyTokenToClipboard)
                .DisposeWith(Disposables);
            ChangePasswordCommand = ReactiveCommand
                .CreateFromTask(ChangePassword)
                .DisposeWith(Disposables);

            OpenUsersCommand = ReactiveCommand
                .Create(shell.ShowUsers)
                .DisposeWith(Disposables);
            OpenRolesCommand = ReactiveCommand
                .Create(shell.ShowRoles)
                .DisposeWith(Disposables);
            OpenPermissionsCommand = ReactiveCommand
                .Create(shell.ShowPermissions)
                .DisposeWith(Disposables);
            OpenApplicationsCommand = ReactiveCommand
                .Create(shell.ShowApplications)
                .DisposeWith(Disposables);
            OpenEnvironmentsCommand = ReactiveCommand
                .Create(shell.ShowEnvironments)
                .DisposeWith(Disposables);
            OpenMessagesCommand = ReactiveCommand
                .Create(shell.ShowMessages)
                .DisposeWith(Disposables);

            AccentColors = ThemeManager.ColorSchemes
                .Select(a => new ColorData(_appConfigStorage,
                    (x, v) => x.AccentColor = v)
                {
                    Name = a.Name,
                    ColorBrush = a.ShowcaseBrush,
                })
                .ToList();
            AppThemes = ThemeManager.Themes
                .GroupBy(x => x.Type)
                .Select(x => x.First())
                .Select(a => new ColorData(_appConfigStorage,
                    (x, v) => x.AppTheme = v)
                {
                    Name = a.BaseColorScheme,
                    BorderColorBrush = a.Resources["MahApps.Brushes.Black"] as Brush, 
                    ColorBrush = a.Resources["MahApps.Brushes.White"] as Brush,
                })
                .ToList();
        }

        public List<ColorData> AccentColors { get; set; }
        public List<ColorData> AppThemes { get; set; }

        public ReactiveCommand<Unit, Unit> LogoutCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CopyTokenToClipboardCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; set; }

        public ReactiveCommand<Unit, Unit> OpenUsersCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenRolesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenPermissionsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenApplicationsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenEnvironmentsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenMessagesCommand { get; set; }


        private void Logout()
        {
            _domain0.Reconnect();
        }

        private Task Reload()
        {
            return _domain0.LoadModel();
        }

        private void CopyTokenToClipboard()
        {
            Clipboard.SetText($"Bearer {_authContext.Token}");
        }

        private async Task ChangePassword()
        {
            try
            {
                var changePasswordDialogData = await _shell.ShowChangePasswordDialog();
                if (changePasswordDialogData == null)
                    return;

                var changePasswordRequest = new ChangePasswordRequest(
                    changePasswordDialogData.PasswordNew,
                    changePasswordDialogData.PasswordOld);
                await _domain0.Client.ChangeMyPasswordAsync(changePasswordRequest);
            }
            catch (Exception e)
            {
                await _shell.HandleException(e, "Failed to Change Password");
            }
        }
    }


    public class ColorData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private readonly Lazy<ReactiveCommand<Unit, Unit>> _changeAccentCommand;
        public ReactiveCommand<Unit, Unit> ChangeAccentCommand => _changeAccentCommand.Value;

        public ColorData(IAppConfigStorage storage, Action<AppConfig, string> action)
        {
            _changeAccentCommand = new Lazy<ReactiveCommand<Unit, Unit>>(() =>
                ReactiveCommand.Create(() =>
                {
                    var config = storage.Load();
                    action(config, Name);
                    storage.Save(config);

                    ThemeManager.ChangeTheme(Application.Current, config.AppTheme, config.AccentColor);
                }));
        }
    }
}
