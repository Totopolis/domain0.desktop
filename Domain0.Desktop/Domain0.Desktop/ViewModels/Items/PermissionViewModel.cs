using System;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class PermissionViewModel : ReactiveObject, IItemViewModel, IDisposable
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int ApplicationId { get; set; }
        [Reactive] public string Description { get; set; }

        [Reactive] public string Application { get; set; }

        public CompositeDisposable Disposables { get; } = new CompositeDisposable();
        public void Dispose()
        {
            Disposables?.Dispose();
        }
    }
}