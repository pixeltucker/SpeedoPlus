// new

using System;
using System.Windows.Input;

// TODO: Find a better way to notify changes than a public OnCanExecuteChanged method
// beware of memory leaks though...

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
        public void OnCanExecuteChanged()
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