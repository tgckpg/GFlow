using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Messaging;

namespace GFlow.Dialogs
{
	using Models.Procedure;

	sealed partial class EditProcResult : Page
	{
		private ProcResult EditTarget;

		public EditProcResult()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );
			if ( e.Parameter is ProcResult EditTarget )
			{
				this.EditTarget = EditTarget;

				LayoutRoot.DataContext = EditTarget;
				KeyInput.Text = EditTarget.Key;
			}
		}

		private void ToggleMode( object sender, RoutedEventArgs e )
		{
			EditTarget.ToggleMode();
		}

		private void SetKey( object sender, RoutedEventArgs e )
		{
			EditTarget.Key = ( sender as TextBox ).Text;
		}

		private void AddOutputDef( object sender, RoutedEventArgs e )
		{
			EditTarget.ProcessNodes.Add( new ProcResult.OutputDef() );
		}

		private void RemoveOutputDef( object sender, RoutedEventArgs e )
		{
			Button B = ( Button ) sender;
			EditTarget.ProcessNodes.Remove( ( ProcResult.OutputDef ) B.DataContext );
		}

		private void SetDefKey( object sender, RoutedEventArgs e )
		{
			TextBox Input = ( TextBox ) sender;
			ProcResult.OutputDef Item = ( ProcResult.OutputDef ) Input.DataContext;
			Item.Key = Input.Text;
			MessageBus.Send( typeof( Pages.GFEditor ), "REDRAW" );
		}

	}
}