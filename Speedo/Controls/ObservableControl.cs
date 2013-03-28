// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Speedo.Controls
{
    public abstract class ObservableControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetProperty<T>( ref T field, T value, [CallerMemberName] string propertyName = "" )
        {
            NotifyHelper.SetProperty( ref field, value, propertyName, this, PropertyChanged );
        }
    }
}