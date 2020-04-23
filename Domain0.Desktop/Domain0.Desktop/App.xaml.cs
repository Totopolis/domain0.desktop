using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using Domain0.Api.Client;
using Domain0.Desktop.Config;
using Domain0.Desktop.Extensions;
using Domain0.Desktop.Services;
using Domain0.Desktop.Views;
using MahApps.Metro;
using Monik.Common;
using Ui.Wpf.Common;
using Ui.Wpf.Common.DockingManagers;
using Ui.Wpf.Common.ShowOptions;
using Application = System.Windows.Application;

namespace Domain0.Desktop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dm = new DefaultDockingManager
            {
                DocumentPaneControlStyle = FindResource("AvalonDockThemeCustomDocumentPaneControlStyle") as Style,
                AnchorablePaneControlStyle = FindResource("AvalonDockThemeCustomAnchorablePaneControlStyle") as Style,
            };
            dm.SetResourceReference(Control.BackgroundProperty, "MahApps.Brushes.White");

            var shell = UiStarter.Start<IDockWindow>(
                new Bootstrap(),
                new UiShowStartWindowOptions
                {
                    Title = "Domain0.Desktop",
                    DockingManager = dm,
                }
            );

            shell.SetContainerWidth(DefaultDockingManager.Tools, new GridLength(60));

            // log trace to monik
            var monik = shell.Container.Resolve<IMonik>();
            Trace.Listeners.Add(new MonikTraceListener(monik));
            // catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.IsTerminating)
                    monik.ApplicationFatal("Unhandled fatal exception: {1}", args.ExceptionObject);
                else
                    monik.ApplicationError("Unhandled exception: {1}", args.ExceptionObject);
            };

            var config = shell.Container.Resolve<IAppConfigStorage>().Load();
            ThemeManager.ChangeTheme(this, config.AppTheme, config.AccentColor);

            shell.ShowTool<ManageToolsView>(new ViewRequest("manage-tools"), new UiShowOptions { Title = "Tools" });
            shell.ShowUsers();

            var domain0 = shell.Container.Resolve<IDomain0Service>();
            var loginService = shell.Container.Resolve<ILoginService>();

            if (config.HostUrl != null && loginService.IsLoggedIn)
            {
                shell.Container.Resolve<IAuthenticationContext>().HostUrl = config.HostUrl;
                domain0.LoadModel();
            }
            else
                loginService.ShowLogin(() => domain0.LoadModel());
        }
    }
}