using AmmySidekick;
using Autofac;
using Domain0.Desktop.Services;
using Domain0.Desktop.Views;
using System;
using System.Windows;
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

            void ShowInitializedScreen()
            {
                shell.ShowTool<ManageToolsView>(new ViewRequest("manage-tools"), new UiShowOptions {Title = "Tools"});

                shell.ShowView<ManageUsersView>(new ViewRequest("manage-users"), new UiShowOptions {Title = "Users"});
                shell.ShowView<ManageMessagesView>(new ViewRequest("manage-messages"), new UiShowOptions {Title = "Messages"});
                shell.ShowView<ManageApplicationsView>(new ViewRequest("manage-applications"), new UiShowOptions {Title = "Applications"});
                shell.ShowView<ManageRolesView>(new ViewRequest("manage-roles"), new UiShowOptions {Title = "Roles"});
                shell.ShowView<ManagePermissionsView>(new ViewRequest("manage-permissions"), new UiShowOptions {Title = "Permissions"});
            }

            var loginService = shell.Container.Resolve<ILoginService>();

            if (!loginService.LoadPreviousToken())
                loginService.ShowLogin(true, ShowInitializedScreen);
            else
                ShowInitializedScreen();
        }
    }
}
