using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class EnvironmentViewModel : ReactiveObject, IItemViewModel
    {
        [Reactive] public string Description { get; set; }
        [Reactive] public int? Id { get; set; }
        [Reactive] public bool IsDefault { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Token { get; set; }
    }
}