using Domain0.Desktop.ViewModels;
using System.Reactive;

namespace Domain0.Desktop.Views
{
    public partial class ManageMessagesView : ViewUserControl
    {
        public ManageMessagesView(ManageMessagesViewModel vm) : base(vm)
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
