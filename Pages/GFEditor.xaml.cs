using System;
using System.Collections.Generic;
using System.IO;
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

namespace GFlow.Pages
{
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

			GFPropertyPanel PropertyPanel = new GFPropertyPanel():
			GFProcedureList CompPanel = new GFProcedureList();

		}

	}
}