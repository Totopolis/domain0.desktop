using Domain0.Desktop.Services;
using Ui.Wpf.Common.ViewModels;

namespace Domain0.Desktop.ViewModels
{
    public class ManageApplicationsViewModel : ViewModelBase
    {

        private readonly IDomain0Service _domain0;

        public ManageApplicationsViewModel(IDomain0Service domain0)
        {
            _domain0 = domain0;
        }



    }
}
