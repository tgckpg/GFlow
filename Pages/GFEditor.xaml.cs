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
			var k = new GFPanel();
			k.Children.Add( new GFButton() { Label = "Item A", MouseOver = BtnOver, MouseOut = BtnOut } );
			k.Children.Add( new GFButton() { Label = "Item B", MouseOver = BtnOver, MouseOut = BtnOut } );
			k.Children.Add( new GFButton() { Label = "Item C", MouseOver = BtnOver, MouseOut = BtnOut } );
			k.Children.Add( new GFButton() { Label = "Item D", MouseOver = BtnOver, MouseOut = BtnOut } );
			k.Children.Add( new GFButton() { Label = "Item E", MouseOver = BtnOver, MouseOut = BtnOut } );
			DBoard.Add( k );

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

	}
}