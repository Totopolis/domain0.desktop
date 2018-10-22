using Domain0.Api.Client;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedItemPermissionViewModel : SelectedItemViewModel<Permission>
    {
        public SelectedItemPermissionViewModel(bool initIsSelected, int initCount, int initTotal)
            : base(initIsSelected, initCount, initTotal)
        {
        }
    }
}