using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Loaders;

using libtaotu.Crawler;
using libtaotu.Models.Procedure;
using libtaotu.Resources;

namespace libtaotu.Dialogs
{
	sealed partial class EditProcUrlList : ContentDialog
	{
		private ProcUrlList EditTarget;

		private EditProcUrlList()
		{
			InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			StringResources stx = new StringResources( "/libtaotu/Message" );
			PrimaryButtonText = stx.Str( "OK" );
		}

		public EditProcUrlList( ProcUrlList procUrlList )
			:this()
		{
			EditTarget = procUrlList;

			IncomingCheck.IsChecked = EditTarget.Incoming;
			DelimitedCheck.IsChecked = EditTarget.Delimited;
			PrefixInput.Text = EditTarget.Prefix;
			UrlList.ItemsSource = EditTarget.Urls;

			UrlInput.KeyDown += UrlInput_KeyDown;
		}

		private void UrlInput_KeyDown( object sender, KeyRoutedEventArgs e )
		{
			if( e.Key == Windows.System.VirtualKey.Enter )
			{
				TryAddUrl();
			}
		}

		private void AddUrl( object sender, RoutedEventArgs e ) { TryAddUrl(); }

		private async void AddRemainingUrl( ContentDialog sender, ContentDialogButtonClickEventArgs args )
		{
			if ( TryAddUrl() )
			{
				args.Cancel = true;

				await Task.Delay( 300 );
				this.Hide();
			}
		}

		private void RemoveUrl( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			string s = B.DataContext as string;
			EditTarget.Urls.Remove( s );

			// Restore input
			UrlInput.Text = s;

			UrlList.ItemsSource = null;
			UrlList.ItemsSource = EditTarget.Urls;
		}

		private async void PreviewUrl( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			string Url = B.DataContext as string;
			Frame.Navigate( Shared.SourceView, await new ProceduralSpider( new Procedure[ 0 ] ).DownloadSource( Url ) );
			FrameContainer.Visibility = Visibility.Visible;
		}

		private void CloseFrame( object sender, RoutedEventArgs e )
		{
			FrameContainer.Visibility = Visibility.Collapsed;
		}

		private bool TryAddUrl()
		{
			string url = UrlInput.Text.Trim();
			if ( string.IsNullOrEmpty( url ) ) return false;

			EditTarget.Urls.Add( url );

			UrlInput.Text = "";
			UrlList.ItemsSource = null;
			UrlList.ItemsSource = EditTarget.Urls;

			return true;
		}

		private void SetPrefix( object sender, RoutedEventArgs e )
		{
			EditTarget.Prefix = ( sender as TextBox ).Text;
		}

		private void SetIncoming( object sender, RoutedEventArgs e )
		{
			EditTarget.Incoming = ( bool ) IncomingCheck.IsChecked;
		}

		private void SetDelimited( object sender, RoutedEventArgs e )
		{
			EditTarget.Delimited = ( bool ) DelimitedCheck.IsChecked;
		}
	}
}