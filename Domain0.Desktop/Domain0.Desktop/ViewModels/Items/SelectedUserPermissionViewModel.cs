using Domain0.Api.Client;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedUserPermissionViewModel : SelectedItemViewModel<Permission>
    {
        public SelectedUserPermissionViewModel(bool initIsSelected, int initCount, int initTotal)
            : base(initIsSelected, initCount, initTotal)
        {
        }
    }
}