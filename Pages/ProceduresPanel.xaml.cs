using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Net.Astropenguin.Messaging;

using libtaotu.Controls;
using libtaotu.Models.Interfaces;
using libtaotu.Models.Procedure;

namespace libtaotu.Pages
{
    public sealed partial class ProceduresPanel : Page
    {
        public static readonly string ID = typeof( ProceduresPanel ).Name;

        private bool Running = false;
        private ProcManager PM;
        private Procedure SelectedItem;

        private ObservableCollection<LogArgs> Logs = new ObservableCollection<LogArgs>();

        public ProceduresPanel()
        {
            this.InitializeComponent();
            SetTemplate();
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

            if ( e.NavigationMode == NavigationMode.Back )
            {
                SelectedItem.Edit();
            }
        }

        private void SetTemplate()
        {
            PM = new ProcManager();

            StringResources stx = new StringResources( "/libtaotu/ProcItems" );
            Dictionary<ProcType, string> ProcChoices = new Dictionary<ProcType, string>();

            Type PType = typeof( ProcType );
            foreach ( ProcType P in Enum.GetValues( PType ) )
            {
                string ProcName = stx.Str( Enum.GetName( PType, P ) );

                if ( string.IsNullOrEmpty( ProcName ) ) continue;

                ProcChoices.Add( P, ProcName );
            }

            ProcComboBox.ItemsSource = ProcChoices;
            ProcSteps.ItemsSource = PM.ProcList;
            RunLog.ItemsSource = Logs;

            MessageBus.OnDelivery += MessageBus_OnDelivery;

            ProcManager.PanelMessage( ID, "Welcome to Procedural Spider's Control", LogType.INFO );

            ReadProcedures();
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

        private void ViewRaw( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;
            SelectedItem.ToXParem();
        }

        private void RemoveProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;

            PM.RemoveProcedure( SelectedItem );
            SelectedItem = null;
        }

        private void ReadProcedures()
        {
            // XXX: Experimental Read/Write Settings
            Net.Astropenguin.IO.XRegistry XReg = new Net.Astropenguin.IO.XRegistry( "<pp />", "Setting/Test.xml" );
            Net.Astropenguin.IO.XParameter Param = XReg.GetParameters().FirstOrDefault();
            if ( Param != null ) PM.ReadParam( Param );
        }

        private void ExportProcedures( object sender, RoutedEventArgs e )
        {
            // XXX: Experimental Read/Write Settings
            Net.Astropenguin.IO.XRegistry XReg = new Net.Astropenguin.IO.XRegistry( "<pp />", "Setting/Test.xml" );
            XReg.SetParameter( PM.ToXParam() );
            XReg.Save();
        }

        private async void RunProcedure( object sender, RoutedEventArgs e )
        {
            if ( Running ) return;
            Running = true;
            await PM.Run();
            Running = false;
        }

        private void ShowProcContext( object sender, RightTappedRoutedEventArgs e )
        {
            Grid G = sender as Grid;
            FlyoutBase.ShowAttachedFlyout( G );
            SelectedItem = G.DataContext as Procedure;
        }

        private async void MessageBus_OnDelivery( Message Mesg )
        {
            if ( Mesg.TargetType != GetType() ) return;

            // Goto SubProcedures Edit
            if ( Mesg.Payload is ISubProcedure )
            {
                Procedure P = Mesg.Payload as Procedure;
                if ( PM.ProcList.Contains( P ) )
                {
                    await Dispatcher.RunIdleAsync( x => {
                        Frame.Navigate( typeof( SubProceduresPanel ), P );
                    } );
                }
            }

            // Append Logs
            else if ( Mesg.Payload is PanelLog )
            {
                PanelLog PLog = ( PanelLog ) Mesg.Payload;
                PanelLogItem( PLog.ID, Mesg.Content, PLog.LogType );
            }
        }

        private void PanelLogItem( string id, string content, LogType level )
        {
            Logs.Add( new LogArgs( id, content, level, Signal.LOG ) );

            while ( 1000 < Logs.Count )
            {
                Logs.RemoveAt( 0 );
            }
        }

        public class PanelLog
        {
            public string ID;
            public LogType LogType;
        }
    }
}
