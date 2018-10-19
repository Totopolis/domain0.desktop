using ReactiveUI;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;

namespace Domain0.Desktop.ViewModels.Items
{
    public class SelectedItemViewModel<T> : ReactiveObject
    {
        private readonly bool _initIsSelected;
        private readonly int _initCount;
        private int _count;

        public SelectedItemViewModel(bool initIsSelected, int initCount, int initTotal)
        {
            _initIsSelected = initIsSelected;
            _initCount = initCount;
            Total = initTotal;

            IsSelected = _initIsSelected;
            Count = initCount;
        }

        public T Item { get; set; }

        [Reactive] public bool IsSelected { get; set; }

        public int Count
        {
            get => _count;
            set
            {
                this.RaiseAndSetIfChanged(ref _count, value);
                // update fields
                Percent = Count == 0 ? 0.3 : ((double) Count / Total + 1) * 0.5;
                AmountString = Count == Total || Count == 0 ? "" : $"{Count}/{Total}";
            }
        }

        public int Total { get; }
        [Reactive] public double Percent { get; private set; }
        [Reactive] public string AmountString { get; private set; }

        public IEnumerable<int> ParentIds { get; set; }

        public bool IsSelectedChanged => _initIsSelected != IsSelected;
        public bool IsChanged => IsSelectedChanged || _initCount != Count;

        public void MakeFull()
        {
            IsSelected = true;
            Count = Total;
        }

        public void MakeEmpty()
        {
            IsSelected = false;
            Count = 0;
        }

        public void Restore()
        {
            IsSelected = _initIsSelected;
            Count = _initCount;
        }
    }
}
