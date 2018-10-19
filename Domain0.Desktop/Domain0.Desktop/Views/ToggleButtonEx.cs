using System.Windows.Controls.Primitives;

namespace Domain0.Desktop.Views
{
    public class ToggleButtonEx : ToggleButton
    {
        // Don't change IsChecked here
        // it is binded to reactive property
        protected override void OnToggle()
        {
        }
    }
}
