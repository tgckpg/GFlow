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

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

using libtaotu.Controls;
using libtaotu.Models.Interfaces;
using libtaotu.Models.Procedure;

namespace libtaotu.Pages
{
    public sealed partial class SubProceduresPanel : Page
    {
        public static readonly string ID = typeof( SubProceduresPanel ).Name;

        ProcManager PM;
        Procedure MasterProcedure;
        Procedure SelectedItem;

        public SubProceduresPanel()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom( NavigationEventArgs e )
        {
            base.OnNavigatedFrom( e );
            Logger.Log( ID, string.Format( "OnNavigatedFrom: {0}", e.SourcePageType.Name ), LogType.INFO );
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            base.OnNavigatedTo( e );
            Logger.Log( ID, string.Format( "OnNavigatedTo: {0}", e.SourcePageType.Name ), LogType.INFO );
            SetTemplate( e.Parameter as Procedure );
        }

        private void SetTemplate( Procedure Proc )
        {
            MasterProcedure = Proc;
            PM = ( Proc as ISubProcedure ).SubProcedures;

            StringResources stx = new StringResources( "/libtaotu/ProcItems" );
            Dictionary<ProcType, string> ProcChoices = new Dictionary<ProcType, string>();

            Type PType = typeof( ProcType );
            foreach( ProcType P in Enum.GetValues( PType ) )
            {
                string ProcName = stx.Str( Enum.GetName( PType, P ) );

                if( P == ProcType.EXTRACT || P == ProcType.MARK ) continue;

                if ( string.IsNullOrEmpty( ProcName ) ) continue;

                ProcChoices.Add( P, ProcName );
            }

            ProcComboBox.ItemsSource = ProcChoices;
            ProcSteps.ItemsSource = PM.ProcList;
        }

        private void AddProcedure( object sender, RoutedEventArgs e )
        {
            if ( ProcComboBox.SelectedItem == null ) return;

            KeyValuePair<ProcType, string> p = ( KeyValuePair<ProcType, string> ) ProcComboBox.SelectedItem;
            PM.NewProcedure( p.Key );
        }

        private async void EditProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;
            await SelectedItem.Edit();
        }

        private void RemoveProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;

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
