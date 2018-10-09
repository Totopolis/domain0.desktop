using System.Windows;
using Domain0.Desktop.ViewModels;

namespace Domain0.Desktop.Views
{
    public partial class ManageUsersView : ViewUserControl
    {
        public ManageUsersView(ManageUsersViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        private void ToggleCreateUser(object sender, RoutedEventArgs e)
        {
            this.flyoutCreateUser.IsOpen = !this.flyoutCreateUser.IsOpen;
        }

    }
}
