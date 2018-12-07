using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace GFlow.Crawler
{
	using Controls;
	using Models.Interfaces;
	using Models.Procedure;
	using Resources;

	sealed class ProceduralSpider : ICrawler
	{
		public static readonly string ID = typeof( ProceduralSpider ).Name;

		private IEnumerable<Procedure> ProcList;

		public Action<string, LogType> Log { get; set; } = ( a, b ) => ProcManager.PanelMessage( ID, a, b );
		public Action<Procedure, string, LogType> PLog { get; set; } = ( p, a, b ) => ProcManager.PanelMessage( p, a, b );

		public Exception LastException { get; private set; }

		public ProceduralSpider( IEnumerable<Procedure> ProcList )
		{
			this.ProcList = ProcList;
		}

		public async Task<ProcConvoy> Crawl( ProcConvoy Convoy = null )
		{
			if ( ProcList.Count() == 0 )
			{
				Log( Res.RSTR( "EmptyCrawling" ), LogType.INFO );
				return Convoy;
			}

			ProcConvoy Conveying = Convoy;

			foreach ( Procedure Proc in ProcList )
			{
				Log( Res.RSTR( "Running" ) + ": " + Proc.Name, LogType.INFO );

				try
				{
					Proc.Running = true;
					ProcConvoy Received = await Proc.Run( this, Conveying );
					Conveying = Received;
					Proc.Running = false;
				}
				catch ( Exception ex )
				{
					LastException = ex;

					Log(
						Res.RSTR(
							"Faulted", Proc.Name
							, ex.Message + ( ex.InnerException == null ? "" : "[ " + ex.InnerException.Message + " ]" )
						)
						, LogType.ERROR
					);
					Proc.Running = false;
					Proc.Faulted = true;
					Conveying = null;
					break;
				}
			}

			Log( Res.RSTR( "RunComplete" ), LogType.INFO );
			return Conveying;
		}

		public async Task<IStorageFile> DownloadSource( string url )
		{
			Log( Res.SSTR( "Download", url ), LogType.INFO );

			TaskCompletionSource<IStorageFile> TCS = new TaskCompletionSource<IStorageFile>();

			try
			{
				HttpRequest Request = Shared.CreateRequest( new Uri( url ) );

				StorageFile SF = await AppStorage.MkTemp();
				Request.OnRequestComplete += async ( DRequestCompletedEventArgs DArgs ) =>
				{
					try
					{
						using ( IRandomAccessStream IRS = await SF.OpenAsync( FileAccessMode.ReadWrite ) )
						{
							await IRS.WriteAsync( DArgs.ResponseBytes.AsBuffer() );
							await IRS.FlushAsync();
						}

						TCS.TrySetResult( SF );
					}
					catch ( Exception ex )
					{
						using ( IRandomAccessStream IRS = await SF.OpenAsync( FileAccessMode.ReadWrite ) )
						{
							await IRS.WriteAsync( Encoding.UTF8.GetBytes( ex.Message ).AsBuffer() );
							await IRS.FlushAsync();
						}

						TCS.TrySetException( ex );
					}
				};

				Request.OpenAsync();
			}
			catch ( UriFormatException )
			{
				Log( Res.SSTR( "InvalidURL", url ), LogType.ERROR );
				TCS.TrySetCanceled();
			}

			return await TCS.Task;
		}

	}
}