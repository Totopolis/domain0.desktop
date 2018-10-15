using Domain0.Api.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Domain0.Desktop.ViewModels.Items
{
    public class RoleViewModel : ReactiveObject, IItemViewModel, IDisposable
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public bool IsDefault { get; set; }
        [Reactive] public string Description { get; set; }

        [Reactive] public IEnumerable<Permission> Permissions { get; set; }
        public string PermissionsString { get; set; }

        public CompositeDisposable Disposables { get; } = new CompositeDisposable();
        public void Dispose()
        {
            Disposables?.Dispose();
        }
    }
}