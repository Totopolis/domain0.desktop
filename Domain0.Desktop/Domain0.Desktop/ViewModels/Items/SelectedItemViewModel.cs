using ReactiveUI;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public abstract class SelectedItemViewModel<T> : ReactiveObject
    {
        private readonly int _initCount;
        private int _count;

        public SelectedItemViewModel(int initCount, int initTotal)
        {
            _initCount = initCount;
            Total = initTotal;

            Count = initCount;
        }

        public T Item { get; set; }
        public abstract int Id { get; }

        [Reactive] public bool IsSelected { get; set; }

        public int Count
        {
            get => _count;
            set
            {
                this.RaiseAndSetIfChanged(ref _count, value);
                // update fields
                Percent = IsEmpty ? 0.3 : ((double) Count / Total + 1) * 0.5;
                AmountString = IsFull || IsEmpty ? "" : $"{Count}/{Total}";
                IsSelected = IsFull;
            }
        }

        public int Total { get; }
        [Reactive] public double Percent { get; private set; }
        [Reactive] public string AmountString { get; private set; }

        public IEnumerable<int> ParentIds { get; set; }

        public bool IsChanged => _initCount != Count;
        public bool IsEmpty => Count == 0;
        public bool IsFull => Count == Total;

        public void MakeFull()
        {
            Count = Total;
        }

        public void MakeEmpty()
        {
            Count = 0;
        }

        public void Restore()
        {
            Count = _initCount;
        }
    }
}
