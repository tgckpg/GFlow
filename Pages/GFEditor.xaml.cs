using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Messaging;

namespace GFlow.Pages
{
	using Controls;
	using Models.Procedure;

	public sealed partial class GFEditor : Page
	{
		GFDrawBoard DBoard;
		ProcManager PM;

		ProcType DragProc;
		ProcType DropProc;

		public GFEditor()
		{
			this.InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			PM = new ProcManager();

			DBoard = new GFDrawBoard( DrawBoard );

			// GFPropertyPanel PropertyPanel = new GFPropertyPanel():
			GFProcedureList CompPanel = new GFProcedureList();
			ProceduresList.DataContext = CompPanel;
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

	}
}