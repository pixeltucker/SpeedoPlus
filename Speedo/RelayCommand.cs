// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Speedo
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand( Action<object> execute, Func<object, bool> canExecute = null )
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public void BindToPropertyChange( INotifyPropertyChanged source, string propertyName )
        {
            source.PropertyChanged += ( s, e ) =>
            {
                if ( e.PropertyName == propertyName )
                {
                    OnCanExecuteChanged();
                }
            };
        }

        #region ICommand implementation
        public bool CanExecute( object parameter )
        {
            return canExecute == null || canExecute( parameter );
        }

        public void Execute( object parameter )
        {
            execute( parameter );
        }

        public event EventHandler CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            var evt = CanExecuteChanged;
            if ( evt != null )
            {
                evt( this, EventArgs.Empty );
            }
        }
        #endregion
    }
}