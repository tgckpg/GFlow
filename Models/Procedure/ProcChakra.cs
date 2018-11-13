using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.Storage;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.UI.Icons;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;

namespace GFlow.Models.Procedure
{
	using Controls;
	using Models.Interfaces;

	class ProcChakra : Procedure
	{
		private static string ChakraScript;

		public int STimeout = 6;

		public string ShortScript
		{
			get
			{
				if ( 512 < Script.Length )
					return Script.Substring( 0, 512 ) + "...";
				return Script;
			}
		}

		private string _script = "";
		public string Script
		{
			get { return _script; }
			set
			{
				if ( value == null ) value = "";
				_script = value;
				NotifyChanged( "ShortScript" );
			}
		}

		protected override Color BgColor { get { return Colors.DodgerBlue; } }
		protected override IconBase Icon { get { return new IconScript() { AutoScale = true }; } }

		public override Type PropertyPage => typeof( Dialogs.EditProcChakra );

		public ProcChakra()
			: base( ProcType.CHAKRA )
		{
			ReadChakraScript();
		}

		public override async Task<ProcConvoy> Run( ICrawler Crawler, ProcConvoy Convoy )
		{
			await base.Run( Crawler, Convoy );

			// Search for usable convoy
			ProcConvoy UsableConvoy;
			if ( !TryGetConvoy( out UsableConvoy, ( P, C ) =>
				C.Payload is IEnumerable<IStorageFile>
				|| C.Payload is string
			) ) return Convoy;

			if ( UsableConvoy.Payload is IEnumerable<IStorageFile> )
			{
				IEnumerable<IStorageFile> ISFs = ( IEnumerable<IStorageFile> ) UsableConvoy.Payload;
				foreach ( IStorageFile ISF in ISFs )
				{
					await ParseHtml( Crawler, ISF );
				}
				return new ProcConvoy( this, UsableConvoy.Payload );
			}

			// This is a string
			return new ProcConvoy( this, await ParseHtml( Crawler, ( string ) UsableConvoy.Payload ) );
		}

		protected async Task ParseHtml( ICrawler Crawler, IStorageFile ISF )
		{
			string Html = await ISF.ReadString();
			Html = await ParseHtml( Crawler, Html );
			await ISF.WriteString( Html );
		}

		protected async Task<string> ParseHtml( ICrawler Crawler, string Html )
		{
			// Just put the entire thing into background
			TaskCompletionSource<string> TCS = new TaskCompletionSource<string>();

			WebView WView = null;
			Task p = Task.Run( () =>
			{
				// Strip useless contents
				Regex R = new Regex( @"<(script|head|style)(?: [^>]+)?>[\s\S]*?</\1>", RegexOptions.Multiline | RegexOptions.IgnoreCase );
				Html = R.Replace( Html, x => "<!--" + x.Groups[ 1 ].Value.ToUpper() + "-->" );

				// Hide the entire body, thus disabling things rendering
				R = new Regex( @"<body(?: [^>]+)?>" );
				Html = R.Replace( Html, "<body style=\"display: none;\">" )
					// Apply the custom script here
					.Replace( "<!--HEAD-->", ChakraScript.Replace( "CUSTOM_SCRIPT_TOKEN", Script ) );

				// WebView wants the UI Thread
				Worker.UIInvoke( () =>
				{
					WView = new WebView( WebViewExecutionMode.SeparateThread );
					BoundEvents( Crawler, WView, TCS );
					WView.NavigateToString( Html );
				} );
			} );

			int i = 0;
			Crawler.PLog( this, Res.RSTR( "SCRIPT_LIVE", STimeout ), LogType.INFO );
			// Give N seconds for the script to run
			Timer Tmr = new Timer( x =>
			{
				if ( STimeout <= ++i )
				{
					TCS.TrySetResult( null );
					Crawler.PLog( this, Res.RSTR( "SCRIPT_TIMED_OUT" ), LogType.INFO );
					return;
				}

				Crawler.PLog( this, Res.RSTR( "SCRIPT_LIVE_D", STimeout - i ), LogType.INFO );
			}, null, 1000, 1000 );

			Html = JsonDecode<string>( await TCS.Task );

			Tmr.Dispose();
			p.AsAsyncAction().Cancel();

			if ( WView != null ) Worker.UIInvoke( () => WView.NavigateToString( "" ) );

			return Html;
		}

		private T JsonDecode<T>( string s )
		{
			if ( string.IsNullOrEmpty( s ) ) return default( T );

			using ( MemoryStream ms = new MemoryStream( Encoding.UTF8.GetBytes( s ) ) )
			{
				DataContractJsonSerializer Serializer = new DataContractJsonSerializer( typeof( T ) );
				return ( T ) Serializer.ReadObject( ms );
			}
		}

		// Ensures the events will only fired once
		private void BoundEvents( ICrawler Crawler, WebView w, TaskCompletionSource<string> TCS )
		{
			TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs> CompleteHandler = null;
			TypedEventHandler<WebView, WebViewLongRunningScriptDetectedEventArgs> LongScript = null;
			WebViewNavigationFailedEventHandler Failed = null;
			NotifyEventHandler Notify = null;

			bool Errored = false;
			CompleteHandler = async ( sender, e ) =>
			{
				w.NavigationCompleted -= CompleteHandler;
				try
				{
					string j = await w.InvokeScriptAsync( "ScriptStart", null );
					switch ( j )
					{
						case "WAIT": break;
						case "ERROR":
							Errored = true;
							Crawler.PLog( this, Res.RSTR( "ScriptError" ), LogType.ERROR );
							TCS.TrySetResult( null );
							break;
						default:
							TCS.TrySetResult( j );
							break;
					}
				}
				catch ( Exception ex )
				{
					Crawler.PLog( this, Res.RSTR( "ScriptError", ex.Message ), LogType.ERROR );
					TCS.TrySetResult( null );
				}
			};

			Notify = ( sender, e ) =>
			{
				w.ScriptNotify -= Notify;
				TCS.TrySetResult( e.Value );
				if( Errored )
				{
					Crawler.PLog( this, JsonDecode<string>( e.Value ), LogType.ERROR );
				}
			};

			Failed = ( sender, e ) =>
			{
				w.NavigationFailed -= Failed;
				TCS.TrySetResult( null );
				Crawler.PLog( this, Res.RSTR( "SCRIPT_NAV_FALIED" ), LogType.INFO );
			};

			LongScript = ( sender, e ) =>
			{
				if ( STimeout <= e.ExecutionTime.TotalSeconds )
				{
					w.LongRunningScriptDetected -= LongScript;
					e.StopPageScriptExecution = true;
				}
			};

			w.NavigationCompleted += CompleteHandler;
			w.ScriptNotify += Notify;
			w.NavigationFailed += Failed;
			w.LongRunningScriptDetected += LongScript;
		}

		public override void ReadParam( XParameter Param )
		{
			base.ReadParam( Param );
			Script = Param.GetValue( "Script" );
		}

		public override XParameter ToXParam()
		{
			XParameter Param = base.ToXParam();
			Param.SetValue( new XKey( "Script", Script ) );

			return Param;
		}

		private async void ReadChakraScript()
		{
			if ( string.IsNullOrEmpty( ChakraScript ) )
			{
				TextReader Text = File.OpenText( "GFlow/Resources/ProcChakraJs.html" );
				ChakraScript = await Text.ReadToEndAsync();
			}
		}

	}
}