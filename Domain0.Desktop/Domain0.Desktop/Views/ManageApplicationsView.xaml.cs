using Domain0.Desktop.ViewModels;
using System.Reactive;

namespace Domain0.Desktop.Views
{
    public partial class ManageApplicationsView : ViewUserControl
    {
        public ManageApplicationsView(ManageApplicationsViewModel vm) : base(vm)
        {
            InitializeComponent();

            vm.CreatedItemInList.RegisterHandler(interaction =>
            {
                ManageDataGrid.ScrollIntoView(interaction.Input);
                ManageDataGrid.SelectedItem = interaction.Input;
                interaction.SetOutput(Unit.Default);
            });
        }
    }
}
