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

using libtaotu.Crawler;
using libtaotu.Models.Procedure;

namespace libtaotu.Dialogs
{
	sealed partial class EditProcParam : ContentDialog
	{
		public static readonly string ID = typeof( EditProcParam ).Name;

		private ProcParameter EditTarget;
		private ProceduralSpider MCrawler;

		private EditProcParam()
		{
			this.InitializeComponent();
			MCrawler = new ProceduralSpider( new Procedure[ 0 ] );
			SetTemplate();
		}

		private void SetTemplate()
		{
			StringResources stx = StringResources.Load( "/libtaotu/Message" );
			PrimaryButtonText = stx.Str( "OK" );
		}

		public EditProcParam( ProcParameter EditTarget )
			: this()
		{
			this.EditTarget = EditTarget;

			if ( EditTarget.ParamDefs.Count == 0 )
			{
				EditTarget.ParamDefs.Add( new ProcParameter.ParamDef( "", "" ) );
			}

			ParamControl.DataContext = EditTarget;
			IncomingCheck.IsChecked = EditTarget.Incoming;

			if ( !string.IsNullOrEmpty( EditTarget.TemplateStr ) )
			{
				TemplateStr.Text = EditTarget.TemplateStr;
			}

			if( !string.IsNullOrEmpty( EditTarget.Caption ) )
			{
				Caption.Text = EditTarget.Caption;
			}

			FormattedOutput.Text = EditTarget.ApplyParams( MCrawler );
		}

		private void AddDef( object sender, RoutedEventArgs e )
		{
			EditTarget.AddDef( new ProcParameter.ParamDef( "", "" ) );
			FormattedOutput.Text = EditTarget.ApplyParams( MCrawler );
		}

		private void RemoveDef( object sender, RoutedEventArgs e )
		{
			Button B = sender as Button;
			EditTarget.RemoveDef( B.DataContext as ProcParameter.ParamDef );
			FormattedOutput.Text = EditTarget.ApplyParams( MCrawler );
		}

		private void ToggleMode( object sender, RoutedEventArgs e )
		{
			EditTarget.ToggleMode();
		}

		private void SetLabel( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcParameter.ParamDef Item = Input.DataContext as ProcParameter.ParamDef;
			Item.Label = Input.Text;
		}

		private void SetDefault( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			ProcParameter.ParamDef Item = Input.DataContext as ProcParameter.ParamDef;
			Item.Default = Input.Text;
			FormattedOutput.Text = EditTarget.ApplyParams( MCrawler );
		}

		private void SetIncoming( object sender, RoutedEventArgs e )
		{
			EditTarget.Incoming = ( bool ) IncomingCheck.IsChecked;
		}

		private void SetTemplateStr( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			EditTarget.TemplateStr = Input.Text;
			FormattedOutput.Text = EditTarget.ApplyParams( MCrawler );
		}

		private void SetCaption( object sender, RoutedEventArgs e )
		{
			TextBox Input = sender as TextBox;
			EditTarget.Caption = Input.Text;
		}
	}
}