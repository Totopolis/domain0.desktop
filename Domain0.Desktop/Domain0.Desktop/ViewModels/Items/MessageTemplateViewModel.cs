﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class MessageTemplateViewModel : ReactiveObject, IItemViewModel
    {
        [Reactive] public int? Id { get; set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public int EnvironmentId { get; set; }
        [Reactive] public string Locale { get; set; }
        [Reactive] public string Name { get; set; } = "";
        [Reactive] public string Template { get; set; } = "";
        [Reactive] public string Type { get; set; }
    }
}
