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
            try
            {
                return new PropertyFilter
                {
                    Name = ((DataGrid)values[0]).Columns[(int)values[1]].SortMemberPath,
                    Filter = (string)values[2]
                };
            }
            catch (Exception e)
            {
                // consume exceptions
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}