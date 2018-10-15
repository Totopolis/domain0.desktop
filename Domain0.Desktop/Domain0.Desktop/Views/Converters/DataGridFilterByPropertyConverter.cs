using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Domain0.Desktop.Views.Converters
{
    public class PropertyFilter
    {
        public string Name { get; set; }
        public string Filter { get; set; }
    }

    public class DataGridFilterByPropertyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var columns = ((DataGrid) values[0]).Columns;
            var index = (int) values[1];

            // check that index is in range
            if (index >= 0 && index < columns.Count)
                return new PropertyFilter
                {
                    Name = columns[index].SortMemberPath,
                    Filter = (string) values[2]
                };
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}