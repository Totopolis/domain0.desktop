using Domain0.Desktop.ViewModels;

namespace Domain0.Desktop.Views
{
    public partial class ManageMessagesView : ViewUserControl
    {
        public ManageMessagesView(ManageMessagesViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
