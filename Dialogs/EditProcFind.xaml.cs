using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

using GFlow.Crawler;
using GFlow.Models.Procedure;
using GFlow.Resources;

namespace GFlow.Dialogs
{
	sealed partial class EditProcFind : Page
	{
		public static readonly string ID = typeof( EditProcFind ).Name;

		private IStorageFile TestContent;

		private ProcFind EditTarget;
		private ProceduralSpider MCrawler;

		public EditProcFind()
		{
			InitializeComponent();
			MCrawler = new ProceduralSpider( new Procedure[ 0 ] );
		}

		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );
			if ( e.Parameter is ProcFind EditTarget )
			{
				this.EditTarget = EditTarget;

				if ( EditTarget.RegexPairs.Count == 0 )
				{
					EditTarget.RegexPairs.Add( new ProcFind.RegItem() );
				}

				RegexControl.DataContext = EditTarget;
				if ( !string.IsNullOrEmpty( EditTarget.TestLink ) )
				{
					TestLink.Text = EditTarget.TestLink;
				}
			}
		}

		private async void PreviewProcess( object sender, RoutedEventArgs e )
		{
			string Url = TestLink.Text.Trim();
			if ( string.IsNullOrEmpty( Url ) ) return;

			try
			{
				TestContent = await MCrawler.DownloadSource( Url );
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.INFO );
				return;
			}

			UpdateTestSubject();
		}

		private void AddRegex( object sender, RoutedEventArgs e )
		{
			EditTarget.RegexPairs.Add( new ProcFind.RegItem() );
		}

		private void ToggleMode( object sender, RoutedEventArgs e )
		{
			EditTarget.ToggleMode();
		}

		private void SetPattern( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
			Item.Pattern = Input.Text;

			Item.Validate( EditTarget.Mode );
		}

		private void SetFormat( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
			Item.Format = Input.Text;

			Item.Validate( EditTarget.Mode );
		}

		private void SetTestLink( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			EditTarget.TestLink = Input.Text;
		}

		private void RemoveRegex( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			EditTarget.RemoveRegex( B.DataContext as ProcFind.RegItem );

			UpdateTestSubject();
		}

		private void ApplyRegex( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			ProcFind.RegItem Item = B.DataContext as ProcFind.RegItem;

			Item.Enabled = !Item.Enabled;

			UpdateTestSubject();
		}

		private async void UpdateTestSubject()
		{
			if ( TestContent == null ) return;
			IStorageFile ISF = await EditTarget.FilterContent( MCrawler, TestContent );
			MessageBus.Send( typeof( Pages.GFEditor ), "PREVIEW", ISF );
		}
	}
}