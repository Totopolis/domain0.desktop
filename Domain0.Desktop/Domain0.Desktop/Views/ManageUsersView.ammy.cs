using Domain0.Desktop.ViewModels;
using Domain0.Desktop.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Domain0.Desktop.Views
{
    public partial class ManageUsersView : ViewUserControl
    {
        public ManageUsersView(ManageUsersViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        private void OnUsersSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid) sender;
            ((ManageUsersViewModel) ViewModel).SelectedItemsIds =
                new HashSet<int>(dataGrid.SelectedItems.Cast<UserProfileViewModel>().Select(x => x.Id.Value));
        }
    }
}