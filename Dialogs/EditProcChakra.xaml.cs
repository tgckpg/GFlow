using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.IO;
using Net.Astropenguin.Messaging;

namespace GFlow.Dialogs
{
	using Models.Procedure;
	using Pages;

	sealed partial class EditProcChakra : Page
	{
		private ProcChakra EditTarget;

		public EditProcChakra()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );

			if ( e.Parameter is ProcChakra )
			{
				EditTarget = ( ProcChakra ) e.Parameter;
				ScriptInput.DataContext = EditTarget;
			}
		}

		private async void OpenScript( object sender, RoutedEventArgs e )
		{
			IStorageFile ISF = await AppStorage.OpenFileAsync( ".js" );
			if ( ISF == null ) return;
			EditTarget.Script = await ISF.ReadString();
		}

		private async void ExportScript( object sender, RoutedEventArgs e )
		{
			IStorageFile ISF = await AppStorage.MkTemp();
			await ISF.WriteString( EditTarget.Script );
			MessageBus.Send( typeof( GFEditor ), "PREVIEW", new Tuple<IStorageFile, string>( ISF, "js" ) );
		}
	}
}