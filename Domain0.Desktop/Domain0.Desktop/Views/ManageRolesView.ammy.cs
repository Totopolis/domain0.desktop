using Domain0.Desktop.ViewModels;
using Domain0.Desktop.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Domain0.Desktop.Views
{
    public partial class ManageRolesView : ViewUserControl
    {
        public ManageRolesView(ManageRolesViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        private void OnRolesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid) sender;
            ((ManageRolesViewModel) ViewModel).SelectedItemsIds =
                new HashSet<int>(dataGrid.SelectedItems.Cast<RoleViewModel>().Select(x => x.Id.Value));
        }
    }
}