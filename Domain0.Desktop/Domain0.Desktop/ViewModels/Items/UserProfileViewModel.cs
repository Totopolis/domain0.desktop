using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class UserProfileViewModel : ReactiveObject, IItemViewModel
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public bool IsLocked { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public decimal? Phone { get; set; }
        [Reactive] public string Email { get; set; }
        [Reactive] public string Description { get; set; }
    }
}