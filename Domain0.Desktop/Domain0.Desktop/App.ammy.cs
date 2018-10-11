using AmmySidekick;
using Autofac;
using Domain0.Desktop.Services;
using Domain0.Desktop.Views;
using System;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Application = System.Windows.Application;

namespace Domain0.Desktop
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();

            RuntimeUpdateHandler.Register(app, "/" + AmmySidekick.Ammy.GetAssemblyName(app) + ";component/App.g.xaml");

            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var shell = UiStarter.Start<IDockWindow>(
                new Bootstrap(),
                new UiShowStartWindowOptions
                {
                    Title = "Domain0.Desktop",
                    ToolPaneWidth = 100
                }
            );

            async void ShowLoadingDialog()
            {
                var domain0 = shell.Container.Resolve<IDomain0Service>();
                await domain0.LoadModel().ConfigureAwait(true);

                ShowInitializedScreen();
            }

            void ShowInitializedScreen()
            {
                shell.ShowTool<ManageToolsView>(new ViewRequest("manage-tools"), new UiShowOptions {Title = "Tools"});

                shell.ShowView<ManageUsersView>(new ViewRequest("manage-users"), new UiShowOptions {Title = "Users"});
                shell.ShowView<ManageRolesView>(new ViewRequest("manage-roles"), new UiShowOptions { Title = "Roles" });
                shell.ShowView<ManagePermissionsView>(new ViewRequest("manage-permissions"), new UiShowOptions { Title = "Permissions" });
                shell.ShowView<ManageApplicationsView>(new ViewRequest("manage-applications"), new UiShowOptions { Title = "Applications" });
                shell.ShowView<ManageMessagesView>(new ViewRequest("manage-messages"), new UiShowOptions {Title = "Messages"});
            }

            var loginService = shell.Container.Resolve<ILoginService>();

            if (!loginService.LoadPreviousToken())
                loginService.ShowLogin(true, ShowLoadingDialog);
            else
                ShowLoadingDialog();
        }
    }
}
