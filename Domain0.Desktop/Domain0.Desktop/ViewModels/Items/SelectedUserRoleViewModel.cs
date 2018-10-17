using Domain0.Api.Client;
using System.Collections.Generic;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedUserRoleViewModel : ISelectedItemViewModel
    {
        public int Id { get; set; }
        public Role Role { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public IEnumerable<int> ParentIds { get; set; }

        public string Name => Role.Name;
        public double Percent => (double) Count / Total;
        public string AmountString => Count == Total ? "" : $"{Count}/{Total}";
    }
}