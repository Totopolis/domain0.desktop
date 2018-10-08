using Domain0.Desktop.ViewModels;
using Ui.Wpf.Common;

namespace Domain0.Desktop.Views
{
    public partial class ManageToolsView : ViewUserControl, IToolView
    {
        public ManageToolsView(ManageToolsViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
