using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.DataModel;

using libtaotu.Controls;
using libtaotu.Models.Procedure;

namespace libtaotu.Pages
{
    public sealed partial class ProceduresPanel : Page
    {
        ProcManager PM;
        Procedure SelectedItem;

        public ProceduresPanel()
        {
            this.InitializeComponent();
            SetTemplate();
        }

        private void SetTemplate()
        {
            PM = new ProcManager();
            ProcComboBox.ItemsSource = GenericData<ProcType>.Convert( Enum.GetValues( typeof( ProcType ) ) );
            ProcSteps.ItemsSource = PM.ProcList;
        }

        private void AddProcedure( object sender, RoutedEventArgs e )
        {
            if ( ProcComboBox.SelectedItem == null ) return;

            GenericData<ProcType> p = ProcComboBox.SelectedItem as GenericData<ProcType>;
            PM.NewProcedure( p.Data );
        }

        private async void EditProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;
            await SelectedItem.Edit();
        }

        private void RemoveProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;

            GenericData<ProcType> p = ProcComboBox.SelectedItem as GenericData<ProcType>;
            PM.RemoveProcedure( SelectedItem );
            SelectedItem = null;
        }

        private void ShowProcContext( object sender, RightTappedRoutedEventArgs e )
        {
            Grid G = sender as Grid;
            FlyoutBase.ShowAttachedFlyout( G );
            SelectedItem = G.DataContext as Procedure;
        }
    }
}
