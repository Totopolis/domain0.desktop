using Autofac;
using AutoMapper;
using Domain0.Api.Client;
using Domain0.Desktop.Services;
using Domain0.Desktop.ViewModels;
using Domain0.Desktop.Views;
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

            builder.RegisterType<Domain0Service>().As<IDomain0Service>().SingleInstance();
            builder.RegisterType<LoginService>().As<ILoginService>();

            builder.RegisterType<ManageUsersView>();
            builder.RegisterType<ManageUsersViewModel>();

            builder.RegisterType<ManageApplicationsView>();
            builder.RegisterType<ManageApplicationsViewModel>();

            builder.RegisterType<ManageMessagesView>();
            builder.RegisterType<ManageMessagesViewModel>();

            builder.RegisterInstance(CreateMapper()).As<IMapper>();

            var container = builder.Build();

            var shell = container.Resolve<IShell>();
            shell.Container = container;

            return shell;
        }


        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MessageTemplate, MessageTemplateViewModel>();
                cfg.CreateMap<MessageTemplateViewModel, MessageTemplate>().ConstructUsing(x =>
                    new MessageTemplate(x.Description, x.Id, x.Locale, x.Name, x.Template, x.Type));
            });
            return config.CreateMapper();
        }

    }
}
