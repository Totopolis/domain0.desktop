using Domain0.Api.Client;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedUserRoleViewModel : SelectedItemViewModel<Role>
    {
        public SelectedUserRoleViewModel(bool initIsSelected, int initCount, int initTotal)
            : base(initIsSelected, initCount, initTotal)
        {
        }
    }
}