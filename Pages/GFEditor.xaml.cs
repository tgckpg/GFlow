using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Controls;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace GFlow.Pages
{
	using Controls;
	using Resources;
	using Models.Procedure;

	public sealed partial class GFEditor : Page, IDisposable
	{
		private static readonly string ID = typeof( GFEditor ).Name;

		GFDrawBoard _DrawBoard;
		GFDrawBoard DBoard
		{
			get => _DrawBoard;
			set
			{
				_DrawBoard?.Dispose();
				_DrawBoard = value;
			}
		}

		Action[] Tabs;
		List<Action> UnRegKeys = new List<Action>();
		Button ActiveTab;

		string DragProc;
		string DropProc;

		bool Running = false;
		int PTabIndex = 0;

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
			GFProcedureList CompPanel = new GFProcedureList();
			ProceduresList.DataContext = CompPanel;

			RunLog.ItemsSource = Logs;

			MessageBus.Subscribe( this, MessageBus_OnDelivery );

			Tabs = new Action[] { ShowProcList, ShowPreview, ShowOutput };

			if ( Application.Current is IKeyboardControl KeyControl )
			{
				UnRegKeys.Add( KeyControl.KeyboardControl.RegisterCombination( x => GFMenu.IsOpen = !GFMenu.IsOpen, VirtualKey.F9 ) );
				UnRegKeys.Add( KeyControl.KeyboardControl.RegisterCombination( x => ToggleFull(), VirtualKey.Control, VirtualKey.K ) );
				UnRegKeys.Add( KeyControl.KeyboardControl.RegisterCombination( x => NextTab(), VirtualKey.Control, VirtualKey.Tab ) );
				UnRegKeys.Add( KeyControl.KeyboardControl.RegisterCombination( x => PrevTab(), VirtualKey.Control, VirtualKey.Shift, VirtualKey.Tab ) );
			}

			StartAutoBackup();
		}

		public void Dispose()
		{
			UnRegKeys.ExecEach( x => x() );
			UnRegKeys.Clear();
			MessageBus.Unsubscribe( this, MessageBus_OnDelivery );
		}

		private void GFEditor_RightTapped( object sender, RightTappedRoutedEventArgs e )
		{
			if ( e.OriginalSource is Windows.UI.Xaml.Shapes.Rectangle )
				return;
			GFMenu.IsOpen = !GFMenu.IsOpen;
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
				BindGFPEvents( GFP );

				DBoard.Add( GFP );
				DropProc = null;
			}
		}

		private void BindGFPEvents( GFProcedure Target )
		{
			Target.OnShowProperty += GFP_OnShowProperty;
			Target.OnTestRun += GFP_OnTestRun;
			Target.OnRemove += GFP_OnRemove;
		}

		private void GFP_OnRemove( GFProcedure Target )
		{
			Target.OnRemove -= GFP_OnRemove;
			Target.OnTestRun -= GFP_OnTestRun;
			Target.OnShowProperty -= GFP_OnShowProperty;
		}

		private void GFP_OnShowProperty( GFProcedure Target )
		{
			ProcMeta.DataContext = Target.Properties;
			ProcName.IsEnabled = true;
			PropertyPanel.Navigate( Target.Properties.PropertyPage, Target.Properties );
		}

		private async void GFP_OnTestRun( GFProcedure Target )
		{
			if ( Running ) return;
			Running = true;

			ShowOutput();

			GFProcedure StartProc = DBoard.Find<GFProcedure>( 1 ).FirstOrDefault( x => x.IsStart ) ?? Target;
			GFPathTracer Tracer = new GFPathTracer( DBoard );
			ProcConvoy Convoy = null;

			try
			{
				ProcManager PM = Tracer.CreateProcManager( StartProc, Target, Target );
				Convoy = await PM.CreateSpider().Crawl();
			}
			catch( Exception ex )
			{
				ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
			}

			Running = false;

			if ( !( Convoy == null || Convoy.Payload == null ) )
			{
				// Do nothing if is IEnumerator but it's empty
				if( Convoy.Payload is System.Collections.IEnumerable x && !x.GetEnumerator().MoveNext() )
				{
					return;
				}

				MessageBus.SendUI( typeof( GFEditor ), "PREVIEW", Convoy );
			}
		}

		private void ProcName_LostFocus( object sender, RoutedEventArgs e )
		{
			DrawBoard.Invalidate();
		}

		private void DrawBoard_DragLeave( object sender, DragEventArgs e ) { DropProc = null; }

		private void ShowProcList()
		{
			ActivateTab( BtnProcList );
			ProceduresList.Visibility = Visibility.Visible;
			PTabIndex = 0;
		}

		private void ShowPreview()
		{
			ActivateTab( BtnPreview );
			Preview.Visibility = Visibility.Visible;
			PTabIndex = 1;
		}

		private void ShowOutput()
		{
			ActivateTab( BtnOutput );
			RunLog.Visibility = Visibility.Visible;
			BtnOutput.FontWeight = FontWeights.Normal;
			PTabIndex = 2;
		}

		private void NextTab() => ( Tabs.ElementAtOrDefault( PTabIndex + 1 ) ?? Tabs.First() ).Invoke();
		private void PrevTab() => ( Tabs.ElementAtOrDefault( PTabIndex - 1 ) ?? Tabs.Last() ).Invoke();

		private void ToggleFull_Click( object sender, RoutedEventArgs e ) => ToggleFull();
		private void ToggleFull()
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

			if ( Mesg.Content == "PREVIEW" )
			{
				var j = Dispatcher.RunIdleAsync( x =>
				{
					ShowPreview();
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

				RunLog.ScrollIntoView( Logs.Last() );
			} );
		}

	}
}