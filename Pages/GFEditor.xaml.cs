using System;
using System.Collections.Generic;
using System.IO;
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

namespace GFlow.Pages
{
	using Controls;
	using Controls.BasicElements;

	public sealed partial class GFEditor : Page
	{
		public GFEditor()
		{
			this.InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			GFDrawBoard DBoard = new GFDrawBoard( DrawBoard );

			GFProcedure GFProc = new GFProcedure( new Models.Procedure.ProcResult() );
			DBoard.Add( GFProc );

			GFProcedure GFProc2 = new GFProcedure( new Models.Procedure.ProcFind() );
			DBoard.Add( GFProc2 );

			// GFPropertyPanel PropertyPanel = new GFPropertyPanel():
			// GFProcedureList CompPanel = new GFProcedureList();
		}

		private void BtnOver( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xFF, 0xFF, 0xFF, 0xFF );
		}

		private void BtnOut( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xF0, 0xF0, 0xF0, 0xF0 );
		}

		private GFButton TestButton( string Label )
		{
			GFButton Btn = new GFButton() { Label = Label, MouseOver = BtnOver, MouseOut = BtnOut };
			Btn.LabelFormat.FontSize = 16;
			return Btn;
		}

	}
}