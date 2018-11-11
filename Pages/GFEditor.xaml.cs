﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
		Button ActiveTab;

		string DragProc;
		string DropProc;

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
			DBoard = new GFDrawBoard( DrawBoard );

			// GFPropertyPanel PropertyPanel = new GFPropertyPanel():
			GFProcedureList CompPanel = new GFProcedureList();
			ProceduresList.DataContext = CompPanel;

			RunLog.ItemsSource = Logs;

			MessageBus.Subscribe( this, MessageBus_OnDelivery );
		}

		private void ProceduresList_DragItemsStarting( object sender, DragItemsStartingEventArgs e )
		{
			if ( e.Items[ 0 ] is KeyValuePair<string, string> PType )
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
			if ( DropProc != null )
			{
				GFProcedure GFP = new GFProcedure( GFProcedureList.Create( DropProc ) );

				Vector2 P = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition.ToVector2();
				Vector2 B = new Vector2( ( float ) Window.Current.Bounds.X, ( float ) Window.Current.Bounds.Y );
				GFP.Bounds.XY = P - B - DBoard.PanOffset;
				GFP.OnShowProperty += GFP_OnShowProperty;
				GFP.OnTestRun += GFP_OnTestRun;
				GFP.OnRemove += GFP_OnRemove;

				DBoard.Add( GFP );
				DropProc = null;
			}
		}

		private void GFP_OnRemove( GFProcedure Target )
		{
			Target.OnRemove -= GFP_OnRemove;
			Target.OnTestRun -= GFP_OnTestRun;
			Target.OnShowProperty -= GFP_OnShowProperty;
		}

		private void GFP_OnShowProperty( GFProcedure Target )
		{
			PropertyPanel.Navigate( Target.Properties.PropertyPage, Target.Properties );
		}

		private async void GFP_OnTestRun( GFProcedure Target )
		{
			if ( Running ) return;
			Running = true;

			ActivateTab( BtnOutput );
			RunLog.Visibility = Visibility.Visible;

			GFProcedure StartProc = DBoard.Find<GFProcedure>( 1 ).FirstOrDefault( x => x.IsStart ) ?? Target;
			GFPathTracer Tracer = new GFPathTracer( DBoard );
			ProcManager PM = Tracer.CreateProcManager( StartProc, Target, Target );

			ProcConvoy Convoy = await PM.CreateSpider().Crawl();
			Running = false;

			if ( Convoy != null )
			{
				MessageBus.SendUI( typeof( GFEditor ), "PREVIEW", Convoy.Payload );
			}
		}

		private void DrawBoard_DragLeave( object sender, DragEventArgs e ) { DropProc = null; }

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

				ProcManager PM = new ProcManager();
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
			else if ( Mesg.Content == "REDRAW" )
			{
				DrawBoard.Invalidate();
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