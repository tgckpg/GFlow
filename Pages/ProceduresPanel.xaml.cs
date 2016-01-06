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

using Net.Astropenguin.Controls;
using Net.Astropenguin.Helpers;
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
    public sealed partial class ProceduresPanel : Page
    {
        public static readonly string ID = typeof( ProceduresPanel ).Name;

        private bool Running = false;

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

        ~ProceduresPanel()
        {
            NavigationHandler.OnNavigatedBack -= StepSubProcedures;
            MessageBus.OnDelivery -= MessageBus_OnDelivery;
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

            ProcManager.PanelMessage( ID, "Welcome to Procedural Spider's Control", LogType.INFO );

            Logs.CollectionChanged += ( s, e ) => ScrollToBottom();

            PM = RootManager;

            ReadProcedures();
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
            XReg.SetParameter( RootManager.ToXParam() );
            XReg.Save();
        }

        private void RunProcedure( object sender, RoutedEventArgs e )
        {
            if ( Running ) return;
            PM.ActiveRange( 0, 0 );
            ProcRun();
        }
        #endregion

        // Run from Message
        private async void ProcRun()
        {
            if ( Running ) return;
            Running = true;
            ProcConvoy Convoy = await PM.Run();
            Running = false;

            MessageBus.SendUI( new Message( GetType(), "RUN_RESULT", Convoy ) );
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
                PM.ActiveRange( 0, PM.ProcList.IndexOf( Mesg.Payload as Procedure ) );
                ProcRun();
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
