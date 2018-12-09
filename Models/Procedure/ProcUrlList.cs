using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Logging;

namespace GFlow.Models.Procedure
{
	using Controls;
	using Models.Interfaces;

	class ProcUrlList : Procedure
	{
		public static readonly string ID = typeof( ProcUrlList ).Name;

		public HashSet<string> Urls { get; private set; }
		public bool Incoming { get; set; }
		public bool Delimited { get; set; }
		public string Prefix { get; set; }

		public override Type PropertyPage => typeof( Dialogs.EditProcUrlList );

		public ProcUrlList()
			: base( ProcType.URLLIST )
		{
			Urls = new HashSet<string>();
			Prefix = "";
		}

		public override async Task<ProcConvoy> Run( ICrawler Crawler, ProcConvoy Convoy )
		{
			await base.Run( Crawler, Convoy );

			HashSet<string> ConvoyUrls = null;

			if ( Incoming )
			{
				Crawler.PLog( this, Res.RSTR( "IncomingCheck" ), LogType.INFO );

				ProcConvoy UsableConvoy = ProcManager.TracePackage(
					Convoy, ( P, C ) =>
					{
						return C.Payload is IEnumerable<IStorageFile>
							|| C.Payload is IEnumerable<string>
							|| C.Payload is IStorageFile
							|| C.Payload is string;
					}
				);

				if ( UsableConvoy != null )
				{
					IEnumerable<string> Payloads;

					switch( UsableConvoy.Payload )
					{
						case IStorageFile ISF:
							Payloads = new string[] { await ISF.ReadString() };
							break;
						case IEnumerable<IStorageFile> CSFs:
							Payloads = await CSFs.Remap( x => x.ReadString() );
							break;
						case string Text:
							Payloads = new string[] { Text };
							break;
						case IEnumerable<string> Texts:
							Payloads = Texts;
							break;
						default:
							throw new ArgumentException( "Unexpected type for UsableConvoy.Payload" );
					}

					if ( Delimited )
					{
						ConvoyUrls = new HashSet<string>( Payloads.Breakdown( x => x.Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ) ) );
					}
					else
					{
						ConvoyUrls = new HashSet<string>( Payloads );
					}

					Payloads = null;
				}
			}

			if ( ConvoyUrls == null && Urls.Count == 0 )
			{
				Crawler.PLog( this, Res.RSTR( "EmptyUrlList" ), LogType.WARNING );
			}

			List<IStorageFile> ISFs = new List<IStorageFile>();

			await DownloadToISFs( Crawler, ISFs, Urls );

			if ( ConvoyUrls != null )
			{
				await DownloadToISFs( Crawler, ISFs, ConvoyUrls );
			}

			return new ProcConvoy( this, ISFs );
		}

		private async Task DownloadToISFs( ICrawler Crawler, IList<IStorageFile> ISFs, IEnumerable<string> Urls )
		{
			foreach ( string u in Urls )
			{
				ISFs.Add( await Crawler.DownloadSource( Prefix + u ) );
			}
		}

		public override void ReadParam( XParameter Param )
		{
			base.ReadParam( Param );

			Incoming = Param.GetBool( "Incoming" );
			Delimited = Param.GetBool( "Delimited" );
			Prefix = Param.GetValue( "Prefix" );

			XParameter[] Params = Param.Parameters( "url" );
			foreach( XParameter P in Params )
			{
				Urls.Add( P.GetValue( "url" ) );
			}
		}

		public override XParameter ToXParam()
		{
			XParameter Param = base.ToXParam();
			int i = 0;

			Param.SetValue( new XKey[] {
				new XKey( "Incoming", Incoming )
				, new XKey( "Delimited", Delimited )
				, new XKey( "Prefix", Prefix )
			} );

			foreach ( string url in Urls )
			{
				Param.SetParameter( ( i++ ) + "", new XKey( "url", url ) );
			}

			return Param;
		}
	}
}