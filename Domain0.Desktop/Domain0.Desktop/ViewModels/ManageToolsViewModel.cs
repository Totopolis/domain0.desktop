using Domain0.Desktop.Properties;
using Domain0.Desktop.Services;
using MahApps.Metro;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Ui.Wpf.Common.ViewModels;

namespace Domain0.Desktop.ViewModels
{
    public class ManageToolsViewModel : ViewModelBase
    {
        private readonly IDomain0Service _domain0;
        private readonly ILoginService _loginService;

        public ManageToolsViewModel(IDomain0Service domain0, ILoginService loginService)
        {
            _domain0 = domain0;
            _loginService = loginService;

            LogoutCommand = ReactiveCommand.Create(Logout);
            ReloadCommand = ReactiveCommand.CreateFromTask(Reload);

            AccentColors = ThemeManager.Accents
                .Select(a => new AccentColorData { Name = a.Name, ColorBrush = a.Resources["AccentBaseColorBrush"] as Brush })
                .ToList();
            AppThemes = ThemeManager.AppThemes
                .GroupBy(x => x.Resources)
                .Select(x => x.First())
                .Select(a => new AppThemeData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                .ToList();
        }

        public List<AccentColorData> AccentColors { get; set; }
        public List<AppThemeData> AppThemes { get; set; }

        public ReactiveCommand LogoutCommand { get; set; }
        public ReactiveCommand ReloadCommand { get; set; }

        private void Logout()
        {
            _domain0.ResetAccessToken();
            _loginService.ShowLogin(false);
        }

        private async Task Reload()
        {
            await _domain0.LoadModel();
        }
    }


    public class AccentColorData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private readonly Lazy<ReactiveCommand> _changeAccentCommand;
        public ReactiveCommand ChangeAccentCommand => _changeAccentCommand.Value;

        public AccentColorData()
        {
            _changeAccentCommand = new Lazy<ReactiveCommand>(() => ReactiveCommand.Create(DoChangeTheme));
        }

        protected virtual void DoChangeTheme()
        {
            Settings.Default.AccentColor = Name;
            Settings.Default.Save();

            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(Settings.Default.AccentColor),
                ThemeManager.GetAppTheme(Settings.Default.AppTheme));
        }
    }

    public class AppThemeData : AccentColorData
    {
        protected override void DoChangeTheme()
        {
            Settings.Default.AppTheme = Name;
            Settings.Default.Save();

            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(Settings.Default.AccentColor),
                ThemeManager.GetAppTheme(Settings.Default.AppTheme));
        }
    }

}
