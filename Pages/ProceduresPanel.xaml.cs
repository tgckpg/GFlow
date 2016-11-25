using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Controls;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;
using Net.Astropenguin.UI;

using libtaotu.Controls;
using libtaotu.Models.Interfaces;
using libtaotu.Models.Procedure;
using libtaotu.Resources;

namespace libtaotu.Pages
{
    public sealed partial class ProceduresPanel : Page, IDisposable
    {
        public static readonly string ID = typeof( ProceduresPanel ).Name;

        private bool Running = false;

        // XXX: Use a proper location
        private string TargetFile = "Settings/Test.xml";

        private ProcManager RootManager;
        private ProcManager PM;
        private Procedure SelectedItem;
        private List<Procedure> ProcChains;

        private ObservableCollection<LogArgs> Logs = new ObservableCollection<LogArgs>();

        public ProceduresPanel()
        {
            this.InitializeComponent();
            if( ProcChains == null )
            {
                ProcChains = new List<Procedure>();
            }

            NavigationHandler.InsertHandlerOnNavigatedBack( StepSubProcedures );
            MessageBus.OnDelivery += MessageBus_OnDelivery;
            SetTemplate();
        }

        ~ProceduresPanel() { Dispose(); }

        public void Dispose()
        {
            NavigationHandler.OnNavigatedBack -= StepSubProcedures;
            MessageBus.OnDelivery -= MessageBus_OnDelivery;
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            base.OnNavigatedTo( e );
            NavigationHandler.InsertHandlerOnNavigatedBack( StepSubProcedures );
            MessageBus.OnDelivery += MessageBus_OnDelivery;

            if ( e.Parameter != null )
            {
                string OpeningFile = ( string ) e.Parameter;

                RootManager = PM = new ProcManager();
                ProcChains.Clear();
                SelectedItem = null;
                UpdateVisualData();
                try
                {
                    ProcManager.PanelMessage( ID, Res.SSTR( "Reading", OpeningFile ), LogType.INFO );
                    ReadProcedures( OpeningFile );
                    ProcManager.PanelMessage( ID, () => Res.RSTR( "ParseOK" ), LogType.INFO );

                    UpdateVisualData();
                    TargetFile = OpeningFile;
                }
                catch ( Exception ex )
                {
                    ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                    ProcManager.PanelMessage( ID, () => Res.RSTR( "InvalidXML" ), LogType.ERROR );
                }
            }
        }

        private void SetTemplate()
        {
            StringResources stx = new StringResources( "/libtaotu/ProcItems" );
            Dictionary<ProcType, string> ProcChoices = new Dictionary<ProcType, string>();

            Type PType = typeof( ProcType );
            foreach ( ProcType P in Enum.GetValues( PType ) )
            {
                string ProcName = stx.Str( Enum.GetName( PType, P ) );

                if ( string.IsNullOrEmpty( ProcName ) ) continue;

                ProcChoices.Add( P, ProcName );
            }

            RootManager = new ProcManager();
            ProcComboBox.ItemsSource = ProcChoices;
            RunLog.ItemsSource = Logs;

            ProcManager.PanelMessage( ID, () => Res.RSTR( "Welcome" ), LogType.INFO );

            Logs.CollectionChanged += ( s, e ) => ScrollToBottom();

            PM = RootManager;

            ReadProcedures( TargetFile );
            UpdateVisualData();
        }

        #region Add / Remove / Edit
        private void AddProcedure( object sender, RoutedEventArgs e )
        {
            if ( ProcComboBox.SelectedItem == null ) return;

            KeyValuePair<ProcType, string> p = ( KeyValuePair<ProcType, string> ) ProcComboBox.SelectedItem;
            PM.NewProcedure( p.Key );
        }

        private void RemoveProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;

            PM.RemoveProcedure( SelectedItem );
            SelectedItem = null;
        }

        private void EditProcedure( object sender, RoutedEventArgs e ) { EditProcedure(); }

        private async void EditProcedure()
        {
            if ( SelectedItem == null ) return;
            await SelectedItem.Edit();
        }

        private async void RenameProcedure( object sender, RoutedEventArgs e )
        {
            if ( SelectedItem == null ) return;
            ContentDialog Dialog = ( ContentDialog ) Activator.CreateInstance( Shared.RenameDialog, SelectedItem );
            await Popups.ShowDialog( Dialog );
        }
        #endregion

        #region Item Controls 
        private void ShowProcContext( object sender, RightTappedRoutedEventArgs e )
        {
            Grid G = sender as Grid;
            FlyoutBase.ShowAttachedFlyout( G );
            SelectedItem = G.DataContext as Procedure;
        }

        private void MoveLeft( object sender, RoutedEventArgs e )
        {
            Button B = sender as Button;
            PM.Move( B.DataContext as Procedure, -1 );
        }

        private void MoveRight( object sender, RoutedEventArgs e )
        {
            Button B = sender as Button;
            PM.Move( B.DataContext as Procedure, 1 );
        }
        #endregion

        #region R/W & Run
        private async void OpenProcedures( object sender, RoutedEventArgs e )
        {
            bool Yes = false;

            StringResources stx = new StringResources( "/libtaotu/Message" );
            MessageDialog Msg = new MessageDialog( stx.Str( "ConfirmDiscard" ) );
            Msg.Commands.Add( new UICommand( stx.Str( "Yes" ), x => Yes = true ) );
            Msg.Commands.Add( new UICommand( stx.Str( "No" ) ) );

            await Popups.ShowDialog( Msg );

            if ( !Yes ) return;
            RootManager = PM = new ProcManager();
            ProcChains.Clear();
            SelectedItem = null;
            UpdateVisualData();

            try
            {
                // Remove the file
                new AppStorage().DeleteFile( TargetFile );

                IStorageFile ISF = await AppStorage.OpenFileAsync( ".xml" );
                if ( ISF == null ) return;

                ProcManager.PanelMessage( ID, Res.RSTR( "Reading", ISF.Name ), LogType.INFO );
                ReadXReg( new XRegistry( await ISF.ReadString(), TargetFile ) );
                ProcManager.PanelMessage( ID, () => Res.RSTR( "ParseOK" ), LogType.INFO );

                UpdateVisualData();
            }
            catch( Exception ex )
            {
                ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                ProcManager.PanelMessage( ID, () => Res.RSTR( "InvalidXML" ), LogType.ERROR );
            }
        }

        private void ReadProcedures( string FileLocation )
        {
            ReadXReg( new XRegistry( "<ProcSpider />", FileLocation ) );
        }

        private void ReadXReg( XRegistry XReg )
        {
            XParameter Param = XReg.Parameters().FirstOrDefault();
            if ( Param != null ) PM.ReadParam( Param );
        }

        private void ExportProcedures( object sender, RoutedEventArgs e )
        {
            XRegistry XReg = new XRegistry( "<ProcSpider />", TargetFile );
            XReg.SetParameter( RootManager.ToXParam() );
            XReg.Save();
            ProcManager.PanelMessage( ID, () => Res.RSTR( "Saved" ), LogType.INFO );
        }

        private async void SaveAs( object sender, RoutedEventArgs e )
        {
            IStorageFile ISF = await AppStorage.SaveFileAsync( "XML", new List<string>() { ".xml" } );
            if ( ISF == null ) return;

            try
            {
                XRegistry XReg = new XRegistry( "<ProcSpider />", null );
                XReg.SetParameter( RootManager.ToXParam() );
                await ISF.WriteString( XReg.ToString() );
                ProcManager.PanelMessage( ID, Res.RSTR( "Saved", ISF.Name ), LogType.INFO );
            }
            catch( Exception ex )
            {
                ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                ProcManager.PanelMessage( ID, () => Res.RSTR( "SaveFailed" ), LogType.ERROR );
            }
        }

        private void RunProcedure( object sender, RoutedEventArgs e )
        {
            if ( Running ) return;
            PM.ActiveRange( 0, 0 );
            ProcRun( false );
        }
        #endregion

        // Run from Message
        private async void ProcRun( bool TestRun )
        {
            if ( Running ) return;
            Running = true;
            ProcConvoy Convoy;
            if ( TestRun )
            {
                Convoy = await PM.CreateSpider().Crawl(
                    new ProcConvoy( new ProcDummy( ProcType.TEST_RUN ), null )
                );
            }
            else
            {
                Convoy = await PM.CreateSpider().Crawl();
            }

            Running = false;

            MessageBus.SendUI( GetType(), "RUN_RESULT", Convoy );
        }

        private void SubEdit( Procedure Proc )
        {
            if ( !ProcChains.Contains( Proc ) )
            {
                ProcChains.Add( Proc );
            }

            PM = ( Proc as ISubProcedure ).SubProcedures;
            UpdateVisualData();
        }

        private string GetNameFromChains()
        {
            string Name = "Start";
            foreach( Procedure P in ProcChains )
            {
                Name += " > " + P.Name;
            }

            return Name;
        }

        private void StepSubProcedures( object sender, XBackRequestedEventArgs e )
        {
            if ( 0 < ProcChains.Count )
            {
                e.Handled = true;

                SelectedItem = ProcChains.Last();
                EditProcedure();

                ProcChains.Remove( SelectedItem );

                if ( 0 < ProcChains.Count )
                {
                    SubEdit( ProcChains.Last() );
                    return;
                }
            }

            if( !e.Handled )
            {
                Dispose();
                return; 
            }

            PM = RootManager;
            UpdateVisualData();
        }

        private void UpdateVisualData()
        {
            NameLevel.Text = GetNameFromChains();
            LayoutRoot.DataContext = PM;
            SubProcInd.State = PM == RootManager
                ? ControlState.Foreatii
                : ControlState.Reovia
                ;
        }

        private async void MessageBus_OnDelivery( Message Mesg )
        {
            if ( Mesg.TargetType != GetType() ) return;

            // Procedure Run
            if( Mesg.Content == "RUN" )
            {
                if ( Running ) return;
                PM.ActiveRange( 0, PM.ProcList.IndexOf( Mesg.Payload as Procedure ) + 1 );
                ProcRun( true );
            }

            // Goto SubProcedures Edit
            else if ( Mesg.Payload is ISubProcedure )
            {
                Procedure P = Mesg.Payload as Procedure;
                if ( PM.ProcList.Contains( P ) )
                {
                    await Dispatcher.RunIdleAsync( x => SubEdit( P ) );
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
#if DEBUG
            Logger.Log( id, content, level );
#endif
            Logs.Add( new LogArgs( id, content, level, Signal.LOG ) );

            while ( 1000 < Logs.Count )
            {
                Logs.RemoveAt( 0 );
            }
        }

        private void ScrollToBottom()
        {
            int selectedIndex = RunLog.Items.Count - 1;
            if ( selectedIndex < 0 ) return;

            RunLog.SelectedIndex = selectedIndex;
            RunLog.UpdateLayout();

            RunLog.ScrollIntoView( RunLog.SelectedItem );
        }

        public class PanelLog
        {
            public string ID;
            public LogType LogType;
        }

    }
}