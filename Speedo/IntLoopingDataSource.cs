// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Speedo
{
    public sealed class IntLoopingDataSource : ILoopingSelectorDataSource
    {
        private readonly int min;
        private readonly int max;
        private readonly int increment;
        private int selectedItem;

        public bool Loop { get; set; }

        public IntLoopingDataSource( int min, int max, int increment )
        {
            this.min = min;
            this.max = max;
            this.increment = increment;
            this.Loop = true;
        }

        #region ILoopingSelectorDataSource implementation
        public object GetNext( object relativeTo )
        {
            int next = (int) relativeTo + increment;
            return next <= max ? next : ( Loop ? (int?) min : null );
        }

        public object GetPrevious( object relativeTo )
        {
            int previous = (int) relativeTo - increment;
            return previous >= min ? previous : ( Loop ? (int?) max : null );
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                int intVal = (int) value;
                if ( selectedItem != intVal )
                {
                    int oldVal = selectedItem;
                    selectedItem = (int) intVal;
                    OnSelectionChanged( oldVal, intVal );
                }
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        private void OnSelectionChanged( object oldValue, object newValue )
        {
            var evt = SelectionChanged;
            if ( evt != null )
            {
                evt( this, new SelectionChangedEventArgs( new[] { oldValue }, new[] { newValue } ) );
            }
        }
        #endregion
    }
}