using Domain0.Api.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class ForceCreateUserRoleViewModel : ReactiveObject
    {
        [Reactive] public Role Role { get; set; }
        [Reactive] public bool IsSelected { get; set; }
    }
}
