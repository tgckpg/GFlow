using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace GFlow.Pages
{
	using Controls;
	using Resources;
	using Models.Procedure;

	public sealed partial class GFEditor : Page
	{
		GFDrawBoard DBoard;
		ProcManager PM;

		Button ActiveTab;

		ProcType DragProc;
		ProcType DropProc;

		bool Running = false;

		ObservableCollection<LogArgs> Logs = new ObservableCollection<LogArgs>();

		public GFEditor()
		{
			this.InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			ActiveTab = BtnProcList;

			PM = new ProcManager();
			DBoard = new GFDrawBoard( DrawBoard );

			// GFPropertyPanel PropertyPanel = new GFPropertyPanel():
			GFProcedureList CompPanel = new GFProcedureList();
			ProceduresList.DataContext = CompPanel;

			RunLog.ItemsSource = Logs;

			MessageBus.Subscribe( this, MessageBus_OnDelivery );
		}

		private void ProceduresList_DragItemsStarting( object sender, DragItemsStartingEventArgs e )
		{
			if ( e.Items[ 0 ] is KeyValuePair<ProcType, string> PType )
			{
				DragProc = PType.Key;
			}
		}

		private void DrawBoard_DragOver( object sender, DragEventArgs e )
		{
			e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
			DropProc = DragProc;
		}

		private void ProceduresList_DragItemsCompleted( ListViewBase sender, DragItemsCompletedEventArgs args )
		{
			if ( 0 < DropProc )
			{
				GFProcedure GFP = new GFProcedure( PM.NewProcedure( DropProc ) );

				Vector2 P = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition.ToVector2();
				Vector2 B = new Vector2( ( float ) Window.Current.Bounds.X, ( float ) Window.Current.Bounds.Y );
				GFP.Bounds.XY = P - B;
				GFP.OnShowProperty += GFP_OnShowProperty;

				DBoard.Add( GFP );
				DropProc = 0;
			}
		}

		private void GFP_OnShowProperty( GFProcedure sender )
		{
			PropertyPanel.Navigate( sender.Properties.PropertyPage, sender.Properties );
		}

		private void DrawBoard_DragLeave( object sender, DragEventArgs e ) { DropProc = 0; }

		private void ProcList_Click( object sender, RoutedEventArgs e )
		{
			if ( sender is Button Btn )
			{
				ActivateTab( Btn );
				ProceduresList.Visibility = Visibility.Visible;
			}
		}

		private void Preview_Click( object sender, RoutedEventArgs e )
		{
			if ( sender is Button Btn )
			{
				ActivateTab( Btn );
				Preview.Visibility = Visibility.Visible;
			}
		}

		private void Output_Click( object sender, RoutedEventArgs e )
		{
			if ( sender is Button Btn )
			{
				ActivateTab( Btn );
				RunLog.Visibility = Visibility.Visible;
				BtnOutput.FontWeight = FontWeights.Normal;
			}
		}

		private void ToggleFull_Click( object sender, RoutedEventArgs e )
		{
			if ( MasterGrid_R0.Height.IsStar )
			{
				MasterGrid_R0.Height = new GridLength( 0 );
				MasterGrid_R2.Height = new GridLength( 1, GridUnitType.Star );
				ContentGrid.Height = double.NaN;
			}
			else
			{
				MasterGrid_R0.Height = new GridLength( 1, GridUnitType.Star );
				MasterGrid_R2.Height = GridLength.Auto;
				ContentGrid.Height = 155;
			}
		}

		private void ActivateTab( Button Btn )
		{
			if ( ActiveTab != null )
			{
				Brush Bg = Btn.Background;
				Btn.Background = ActiveTab.Background;
				ActiveTab.Background = Bg;
			}

			ProceduresList.Visibility
				= RunLog.Visibility
				= Preview.Visibility
				= Visibility.Collapsed;

			ActiveTab = Btn;
		}

		private void MessageBus_OnDelivery( Message Mesg )
		{
			if ( Mesg.TargetType != GetType() ) return;

			// Procedure Run
			if ( Mesg.Content == "RUN" )
			{
				if ( Running ) return;
				PM.ActiveRange( 0, PM.ProcList.IndexOf( Mesg.Payload as Procedure ) + 1 );
				// ProcRun( true );
			}
			else if ( Mesg.Content == "PREVIEW" )
			{
				var j = Dispatcher.RunIdleAsync( x =>
				{
					ActivateTab( BtnPreview );
					Preview.Visibility = Visibility.Visible;
					Preview.Navigate( Shared.SourceView, Mesg.Payload );
				} );
			}
			// Append Logs
			else if ( Mesg.Payload is PanelLog PLog )
			{
				PanelLogItem( PLog.ID, Mesg.Content, PLog.LogType );
			}
		}

		private void PanelLogItem( string id, string content, LogType level )
		{
			var j = Dispatcher.RunIdleAsync( x => {
				Logs.Add( new LogArgs( id, content, level, Signal.LOG ) );

				if ( RunLog.Visibility != Visibility.Visible )
				{
					BtnOutput.FontWeight = FontWeights.Bold;
				}

				while ( 1000 < Logs.Count )
				{
					Logs.RemoveAt( 0 );
				}
			} );
		}

	}
}