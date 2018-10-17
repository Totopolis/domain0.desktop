using System.Collections.Generic;

namespace Domain0.Desktop.ViewModels.Items
{
    public interface ISelectedItemViewModel
    {
        int Id { get; set; }
        IEnumerable<int> ParentIds { get; set; }

        string Name { get; }
        double Percent { get; }
        string AmountString { get; }
    }
}
