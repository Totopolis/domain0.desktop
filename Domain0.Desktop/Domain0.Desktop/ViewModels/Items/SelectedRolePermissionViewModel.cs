using Domain0.Api.Client;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedRolePermissionViewModel : SelectedItemViewModel<Permission>
    {
        public SelectedRolePermissionViewModel(bool initIsSelected, int initCount, int initTotal)
            : base(initIsSelected, initCount, initTotal)
        {
        }
    }
}