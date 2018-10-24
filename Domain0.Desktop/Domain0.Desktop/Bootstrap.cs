using Autofac;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Properties;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels;
using Domain0.Desktop.ViewModels.Items;
using Domain0.Desktop.Views;
using Monik.Client;
using Monik.Common;
using Ui.Wpf.Common;

namespace Domain0.Desktop
{
    public class Bootstrap : IBootstraper
    {
        public IShell Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Shell>().As<IShell>().SingleInstance();
            builder.RegisterType<MainWindow>().As<IDockWindow>().SingleInstance();

            builder.RegisterType<Domain0AuthenticationContext>().As<IDomain0AuthenticationContext>().SingleInstance();

            builder.RegisterType<Domain0Service>().As<IDomain0Service>().SingleInstance();
            builder.RegisterType<LoginService>().As<ILoginService>();

            builder.RegisterType<ManageToolsView>();
            builder.RegisterType<ManageToolsViewModel>();

            builder.RegisterType<ManageUsersView>();
            builder.RegisterType<ManageUsersViewModel>();

            builder.RegisterType<ManageApplicationsView>();
            builder.RegisterType<ManageApplicationsViewModel>();

            builder.RegisterType<ManageMessagesView>();
            builder.RegisterType<ManageMessagesViewModel>();

            builder.RegisterType<ManageRolesView>();
            builder.RegisterType<ManageRolesViewModel>();

            builder.RegisterType<ManagePermissionsView>();
            builder.RegisterType<ManagePermissionsViewModel>();

            builder.RegisterInstance(CreateMapper()).As<IMapper>();


            builder.RegisterInstance(new AzureSender(
                    Settings.Default.MonikConnectionString,
                    Settings.Default.MonikQueueName))
                .As<IMonikSender>();
            builder.RegisterInstance(new ClientSettings()
            {
                SourceName = Settings.Default.MonikSourceName,
                InstanceName = Settings.Default.MonikInstanceName,
                AutoKeepAliveEnable = true
            }).As<IMonikSettings>();
            builder.RegisterType<MonikClient>().As<IMonik>().SingleInstance();

            var container = builder.Build();

            var shell = container.Resolve<IShell>();
            shell.Container = container;

            return shell;
        }


        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MessageTemplateViewModel, MessageTemplateViewModel>();
                cfg.CreateMap<MessageTemplate, MessageTemplateViewModel>();
                cfg.CreateMap<MessageTemplateViewModel, MessageTemplate>().ConstructUsing(x =>
                    new MessageTemplate(x.Description, x.Id, x.Locale, x.Name, x.Template, x.Type));

                cfg.CreateMap<ApplicationViewModel, ApplicationViewModel>();
                cfg.CreateMap<Application, ApplicationViewModel>();
                cfg.CreateMap<ApplicationViewModel, Application>().ConstructUsing(x =>
                    new Application(x.Description, x.Id, x.Name));

                cfg.CreateMap<UserProfileViewModel, UserProfileViewModel>();
                cfg.CreateMap<UserProfile, UserProfileViewModel>();
                cfg.CreateMap<UserProfileViewModel, UserProfile>().ConstructUsing(x =>
                    new UserProfile(x.Description, x.Email, x.Id ?? 0, x.IsLocked, x.Name, x.Phone));

                cfg.CreateMap<RoleViewModel, RoleViewModel>();
                cfg.CreateMap<Role, RoleViewModel>();
                cfg.CreateMap<RoleViewModel, Role>().ConstructUsing(x =>
                    new Role(x.Description, x.Id ?? 0, x.IsDefault, x.Name));

                cfg.CreateMap<PermissionViewModel, PermissionViewModel>();
                cfg.CreateMap<Permission, PermissionViewModel>();
                cfg.CreateMap<PermissionViewModel, Permission>().ConstructUsing(x =>
                    new Permission(x.ApplicationId, x.Description, x.Id, x.Name));
            });
            return config.CreateMapper();
        }

    }
}
