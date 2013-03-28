// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Long Zheng, Solal Pirelli

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