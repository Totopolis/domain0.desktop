using Domain0.Desktop.Properties;
using Domain0.Desktop.Services;
using ReactiveUI;
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
        }

        public ReactiveCommand LogoutCommand { get; set; }

        private void Logout()
        {
            _domain0.ResetAccessToken();
            _loginService.ShowLogin();
        }
    }

}
