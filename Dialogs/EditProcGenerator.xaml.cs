using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace libtaotu.Dialogs
{
	using Crawler;
	using Controls;
	using Models.Procedure;

	sealed partial class EditProcGenerator : ContentDialog
	{
		public static readonly string ID = typeof( EditProcFind ).Name;

		private ProcGenerator EditTarget;

		private EditProcGenerator()
		{
			this.InitializeComponent();
			SetTemplate();
		}

		private void SetTemplate()
		{
			StringResources stx = StringResources.Load( "/libtaotu/Message" );
			PrimaryButtonText = stx.Str( "OK" );
		}

		public EditProcGenerator( ProcGenerator EditTarget )
			:this()
		{
			this.EditTarget = EditTarget;

			if( EditTarget.StopIfs.Count == 0 )
			{
				EditTarget.StopIfs.Add( new ProcFind.RegItem( "", "", true ) );
			}

			if( EditTarget.NextIfs.Count == 0 )
			{
				EditTarget.NextIfs.Add( new ProcFind.RegItem( "", "", true ) );
			}

			RegexControl.DataContext = EditTarget;

			IncomingCheck.IsChecked = EditTarget.Incoming;

			if ( !string.IsNullOrEmpty( EditTarget.EntryPoint ) )
			{
				EntryPoint.Text = EditTarget.EntryPoint;
			}
		}

		private async void PreviewProcess( object sender, RoutedEventArgs e )
		{
			string Url = EntryPoint.Text.Trim();
			if ( string.IsNullOrEmpty( Url ) ) return;

			try
			{
				Procedure[] PList = new Procedure[ 1 ];
				PList[ 0 ] = EditTarget;

				await new ProceduralSpider( PList ).Crawl();
			}
			catch( Exception ex )
			{
				ProcManager.PanelMessage( ID, ex.Message, LogType.INFO );
			}
		}

		private void AddNexts( object sender, RoutedEventArgs e )
		{
			EditTarget.NextIfs.Add( new ProcFind.RegItem( "", "", true ) );
		}

		private void AddStops( object sender, RoutedEventArgs e )
		{
			EditTarget.StopIfs.Add( new ProcFind.RegItem( "", "", true ) );
		}

		private void SetPattern( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
			Item.Pattern = Input.Text;

			Item.Validate( FindMode.MATCH );
		}

		private void SetFormat( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
			Item.Format = Input.Text;

			Item.Validate( FindMode.MATCH );
		}

		private void SetEntryPoint( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			EditTarget.EntryPoint = Input.Text;
		}

		private void FirstStopSkip( object sender, RoutedEventArgs e )
		{
			Button Input = sender as Button;
			EditTarget.FirstStopSkip = !EditTarget.FirstStopSkip;
		}

		private void DiscardUnmatched( object sender, RoutedEventArgs e )
		{
			Button Input = sender as Button;
			EditTarget.DiscardUnmatched = !EditTarget.DiscardUnmatched;
		}

		private void RemoveNextRegex( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			EditTarget.NextIfs.Remove( B.DataContext as ProcFind.RegItem );
		}

		private void RemoveStopRegex( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			EditTarget.StopIfs.Remove( B.DataContext as ProcFind.RegItem );
		}

		private void SetIncoming( object sender, RoutedEventArgs e )
		{
			EditTarget.Incoming = ( bool ) IncomingCheck.IsChecked;
		}
	}
}