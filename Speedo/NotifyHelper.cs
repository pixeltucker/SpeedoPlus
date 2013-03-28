// new

using System.ComponentModel;

namespace Speedo
{
    public static class NotifyHelper
    {
        public static void OnPropertyChanged( string propertyName, INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( handler != null )
            {
                handler( source, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public static void SetProperty<T>( ref T field, T value, string propertyName, INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                OnPropertyChanged( propertyName, source, handler );
            }
        }
    }
}