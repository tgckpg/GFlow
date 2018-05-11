using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace libtaotu.Models.Procedure
{
	using Controls;
	using Crawler;
	using Models.Interfaces;

	class ProcUrlList : Procedure
	{
		public static readonly string ID = typeof( ProcUrlList ).Name;

		public HashSet<string> Urls { get; private set; }
		public bool Incoming { get; set; }
		public bool Delimited { get; set; }
		public string Prefix { get; set; }

		protected override IconBase Icon { get { return new IconTOC() { AutoScale = true }; } }
		protected override Color BgColor { get { return Colors.Brown; } }

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
					ConvoyUrls = new HashSet<string>();
					IEnumerable<string> Payloads;

					if ( UsableConvoy.Payload is IEnumerable<IStorageFile> )
					{
						IEnumerable<IStorageFile> CSFs = ( IEnumerable<IStorageFile> ) UsableConvoy.Payload;
						List<string> Defs = new List<string>();

						foreach ( IStorageFile CSF in CSFs )
						{
							Defs.Add( await CSF.ReadString() );
						}

						Payloads = Defs;
					}
					else if ( UsableConvoy.Payload is IStorageFile )
					{
						Payloads = new string[] { await ( ( IStorageFile ) UsableConvoy.Payload ).ReadString() };
					}
					else if ( UsableConvoy.Payload is string )
					{
						Payloads = new string[] { ( string ) UsableConvoy.Payload };
					}
					else
					{
						Payloads = ( IEnumerable<string> ) UsableConvoy.Payload;
					}

					if ( Delimited )
					{
						foreach ( string Urls in Payloads )
							foreach ( string Url in Urls.Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ) )
							{
								ConvoyUrls.Add( Url );
							}
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

		public override async Task Edit()
		{
			await Popups.ShowDialog( new Dialogs.EditProcUrlList( this ) );
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