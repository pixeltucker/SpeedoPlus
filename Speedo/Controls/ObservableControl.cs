// new

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Speedo.Controls
{
    /// <summary>
    /// A helper base class to implement INotifyPropertyChanged in a nice way.
    /// </summary>
    public abstract class ObservableControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected void FirePropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            var evt = this.PropertyChanged;
            if ( evt != null )
            {
                evt( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        /// <summary>
        /// Sets the specified field to the specified value and raises <see cref="PropertyChanged"/> if needed.
        /// </summary>
        protected void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                this.FirePropertyChanged( propertyName );
            }
        }
    }
}