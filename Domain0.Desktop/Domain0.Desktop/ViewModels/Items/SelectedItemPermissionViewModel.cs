using Domain0.Api.Client;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedItemPermissionViewModel : SelectedItemViewModel<Permission>
    {
        public SelectedItemPermissionViewModel(int initCount, int initTotal)
            : base(initCount, initTotal)
        {
        }

        public override int Id => Item.Id.Value;
    }
}