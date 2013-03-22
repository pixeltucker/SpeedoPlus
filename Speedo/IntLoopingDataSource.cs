// new

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

        public IntLoopingDataSource( int min, int max, int increment )
        {
            this.min = min;
            this.max = max;
            this.increment = increment;
            this.Loop = true;
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        private void OnSelectionChanged( object oldValue, object newValue )
        {
            var evt = SelectionChanged;
            if ( evt != null )
            {
                var e = new SelectionChangedEventArgs( new[] { oldValue }, new[] { newValue } );
                evt( this, e );
            }
        }
    }
}