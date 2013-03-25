// new

using System.ComponentModel;

// TODO: Is that really the best way?

namespace Speedo
{
    public static class NotifyHelper
    {
        public static void SetProperty<T>( ref T field, T value, string propertyName, INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( !object.Equals( field, value ) )
            {
                field = value;
                if ( handler != null )
                {
                    handler( source, new PropertyChangedEventArgs( propertyName ) );
                }
            }
        }
    }
}