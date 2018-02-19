using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Messaging;

namespace libtaotu.Dialogs
{
	using Models.Procedure;
	using Pages;
	using Resources;

	sealed partial class EditProcChakra : ContentDialog
	{
		private ProcChakra EditTarget;

		public EditProcChakra()
		{
			this.InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			StringResources stx = new StringResources( "/libtaotu/Message" );
			PrimaryButtonText = stx.Str( "OK" );

			MessageBus.Subscribe( this, MessageBus_OnDelivery );
		}

		public EditProcChakra( ProcChakra EditTarget )
			:this()
		{
			this.EditTarget = EditTarget;
			ScriptInput.DataContext = EditTarget;
		}

		private void RunTilHere( object sender, RoutedEventArgs e )
		{
			TestRunning.IsActive = true;
			MessageBus.SendUI( typeof( ProceduresPanel ), "RUN", EditTarget );
		}

		private async void OpenScript( object sender, RoutedEventArgs e )
		{
			IStorageFile ISF = await AppStorage.OpenFileAsync( ".js" );
			if ( ISF == null ) return;
			EditTarget.Script = await ISF.ReadString();
		}

		private async void ExportScript( object sender, RoutedEventArgs e )
		{
			IStorageFile ISF = await AppStorage.SaveFileAsync( "script", new string[] { ".js" } );
			if ( ISF == null ) return;
			await ISF.WriteString( EditTarget.Script );
		}

		private void MessageBus_OnDelivery( Message Mesg )
		{
			ProcConvoy Convoy = Mesg.Payload as ProcConvoy;
			if ( Mesg.Content == "RUN_RESULT"
				&& Convoy != null
				&& Convoy.Dispatcher == EditTarget )
			{
				TestRunning.IsActive = false;

				IEnumerable<IStorageFile> ISF = Convoy.Payload as IEnumerable<IStorageFile>;
				if( ISF != null )
				{
					Preview.Navigate( Shared.SourceView, ISF.First() );
				}
			}
		}
	}
}