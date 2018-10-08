using ReactiveUI;
using System.Collections.Generic;

namespace Domain0.Desktop.Extensions
{
    public static class ReactiveListExtenstions
    {
        public static void Initialize<T>(this ReactiveList<T> list, IEnumerable<T> value)
        {
            using (list.SuppressChangeNotifications())
            {
                list.Clear();
                list.AddRange(value);
            }
        }
    }
}
