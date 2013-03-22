using System.Windows.Controls;
using System.Windows.Input;

namespace Speedo.Controls
{
    public class BubbleButton : Button
    {
        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            base.OnMouseLeftButtonDown( e );
            e.Handled = false;
        }

        protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            base.OnMouseLeftButtonUp( e );
            e.Handled = false;
        }
    }
}