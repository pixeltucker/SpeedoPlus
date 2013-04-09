// This code is licensed under the Microsoft Reciprocal License (MS-RL). See the LICENSE file for details.
// Contributors: Solal Pirelli

using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Speedo.Controls
{
    /// <summary>
    /// A simple number picker with two digits.
    /// </summary>
    public partial class NumberPicker : ObservableControl
    {
        #region Tens/UnitsSource DependencyProperties
        public ILoopingSelectorDataSource TensSource
        {
            get { return (ILoopingSelectorDataSource) GetValue( TensSourceProperty ); }
            set { SetValue( TensSourceProperty, value ); }
        }

        public static readonly DependencyProperty TensSourceProperty =
            DependencyProperty.Register( "TensSource", typeof( ILoopingSelectorDataSource ), typeof( NumberPicker ), new PropertyMetadata( OnSourcePropertyChanged ) );

        public ILoopingSelectorDataSource UnitsSource
        {
            get { return (ILoopingSelectorDataSource) GetValue( UnitsSourceProperty ); }
            set { SetValue( UnitsSourceProperty, value ); }
        }

        public static readonly DependencyProperty UnitsSourceProperty =
           DependencyProperty.Register( "UnitsSource", typeof( ILoopingSelectorDataSource ), typeof( NumberPicker ), new PropertyMetadata( OnSourcePropertyChanged ) );

        private static void OnSourcePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var picker = (NumberPicker) obj;
            if ( args.OldValue != null )
            {
                ( (ILoopingSelectorDataSource) args.OldValue ).SelectionChanged -= picker.Source_SelectionChanged;
            }
            if ( args.NewValue != null )
            {
                ( (ILoopingSelectorDataSource) args.NewValue ).SelectionChanged += picker.Source_SelectionChanged;
            }
        }
        #endregion

        #region Value DependencyProperty
        public int Value
        {
            get { return (int) GetValue( ValueProperty ); }
            set { SetValue( ValueProperty, value ); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register( "Value", typeof( int ), typeof( NumberPicker ), new PropertyMetadata( OnValuePropertyChanged ) );

        private static void OnValuePropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            var picker = (NumberPicker) obj;
            int intVal = (int) args.NewValue;
            picker.TensSource.SelectedItem = intVal / 10;
            picker.UnitsSource.SelectedItem = intVal % 10;
        }
        #endregion

        public NumberPicker()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void Source_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Value = (int) TensSource.SelectedItem * 10 + (int) UnitsSource.SelectedItem;
        }
    }
}