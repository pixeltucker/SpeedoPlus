using System;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

// TODO fix the code duplication

namespace Speedo
{
    public class Speeds : ILoopingSelectorDataSource
    {
        private int minimum = 0;
        private int maximum = 24;
        private int selectedItem = 6;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        protected virtual void OnSelectedChanged( SelectionChangedEventArgs e )
        {
            var evt = SelectionChanged;

            if ( evt != null )
            {
                evt( this, e );
            }
        }

        public object GetNext( object relativeTo )
        {
            var nextValue = ( (int) relativeTo ) + 1;
            return nextValue <= Maximum ? nextValue : Minimum;
        }

        public object GetPrevious( object relativeTo )
        {
            var previousValue = ( (int) relativeTo ) - 1;
            return previousValue >= Minimum ? previousValue : Maximum;
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                var oldValue = selectedItem;
                var newValue = (int) value;

                if ( oldValue == newValue )
                    return;

                selectedItem = newValue;

                OnSelectedChanged( new SelectionChangedEventArgs( new[] { oldValue }, new[] { newValue } ) );
            }
        }

        public int Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                if ( selectedItem < minimum )
                {
                    SelectedItem = value;
                }
            }
        }


        public int Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                if ( selectedItem > maximum )
                {
                    SelectedItem = value;
                }
            }
        }
    }

    public class Speeds5 : ILoopingSelectorDataSource
    {
        private int minimum = 0;
        private int maximum = 5;
        private int selectedItem = 0;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        protected virtual void OnSelectedChanged( SelectionChangedEventArgs e )
        {
            var evt = SelectionChanged;

            if ( evt != null )
            {
                evt( this, e );
            }
        }

        public object GetNext( object relativeTo )
        {
            var nextValue = ( (int) relativeTo ) + 5;

            if ( nextValue > Maximum )
            {
                return null;
            }
            return nextValue;
        }

        public object GetPrevious( object relativeTo )
        {
            var previousValue = ( (int) relativeTo ) - 5;
            if ( previousValue < Minimum )
            {
                return null;
            }
            return previousValue;
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                var oldValue = selectedItem;
                var newValue = (int) value;

                if ( oldValue == newValue )
                {
                    return;
                }

                selectedItem = newValue;

                OnSelectedChanged( new SelectionChangedEventArgs( new[] { oldValue }, new[] { newValue } ) );
            }
        }

        public int Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                if ( selectedItem < minimum )
                {
                    SelectedItem = value;
                }
            }
        }


        public int Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                if ( selectedItem > maximum )
                {
                    SelectedItem = value;
                }
            }
        }
    }
}