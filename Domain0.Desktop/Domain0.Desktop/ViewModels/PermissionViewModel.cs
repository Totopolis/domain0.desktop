﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels
{
    public class PermissionViewModel : ReactiveObject, IItemViewModel
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int ApplicationId { get; set; }
        [Reactive] public string Description { get; set; }
    }
}