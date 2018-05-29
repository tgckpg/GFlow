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

using GFlow.Crawler;
using GFlow.Models.Procedure;
using GFlow.Resources;
using Windows.Storage;
using Net.Astropenguin.Messaging;

namespace GFlow.Dialogs
{
	sealed partial class EditProcUrlList : Page
	{
		private ProcUrlList EditTarget;
		string TargetUrl;

		public EditProcUrlList()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );
			if( e.Parameter is ProcUrlList )
			{
				EditTarget = ( ProcUrlList ) e.Parameter;
				SetTarget();
			}
		}

		private void SetTarget()
		{
			IncomingCheck.IsOn = EditTarget.Incoming;
			DelimitedCheck.IsOn = EditTarget.Delimited;
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

		private void AddRemainingUrl( ContentDialog sender, ContentDialogButtonClickEventArgs args )
		{
			if ( TryAddUrl() )
			{
				args.Cancel = true;
			}
		}

		private void RemoveUrl( object sender, RoutedEventArgs e )
		{
			EditTarget.Urls.Remove( TargetUrl );

			// Restore input
			UrlInput.Text = TargetUrl;

			UrlList.ItemsSource = null;
			UrlList.ItemsSource = EditTarget.Urls;
		}

		private async void PreviewUrl( object sender, RoutedEventArgs e )
		{
			try
			{
				IStorageFile ISF = await new ProceduralSpider( new Procedure[ 0 ] ).DownloadSource( TargetUrl );
				if ( ISF != null )
				{
					MessageBus.Send( typeof( Pages.GFEditor ), "PREVIEW", ISF );
				}
			}
			catch ( Exception ) { }
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
			EditTarget.Incoming = IncomingCheck.IsOn;
		}

		private void SetDelimited( object sender, RoutedEventArgs e )
		{
			EditTarget.Delimited = DelimitedCheck.IsOn;
		}

		private void Border_RightTapped( object sender, RightTappedRoutedEventArgs e )
		{
			if ( sender is FrameworkElement Element )
			{
				FlyoutBase.ShowAttachedFlyout( Element );
				TargetUrl = ( string ) Element.DataContext;
			}
		}

	}
}