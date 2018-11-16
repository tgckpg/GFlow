using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;

namespace GFlow.Models.Procedure
{
	using Controls;
	using Models.Interfaces;

	class ProcEncoding : Procedure
	{
		public static readonly string ID = typeof( ProcEncoding ).Name;

		public int CodePage { get; set; }
		public bool DecodeHtml { get; set; }

		public override Type PropertyPage => typeof( Dialogs.EditProcEncoding );

		public ProcEncoding()
			: base( ProcType.ENCODING )
		{
			CodePage = Encoding.UTF8.CodePage;
			DecodeHtml = false;
		}

		public override async Task<ProcConvoy> Run( ICrawler Crawler, ProcConvoy Convoy )
		{
			await base.Run( Crawler, Convoy );

			// Search for usable convoy
			ProcConvoy UsableConvoy;
			if ( !TryGetConvoy( out UsableConvoy, ( P, C ) =>
			{
				return C.Payload is IEnumerable<IStorageFile>
				|| C.Payload is string;
			}
			) ) return Convoy;

			try
			{
				if ( DoNothing() ) return new ProcConvoy( this, null );

				Encoding Enc = null;

				if ( CodePage != Encoding.UTF8.CodePage )
				{
					Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
					Enc = Encoding.GetEncoding( CodePage );
				}

				if ( UsableConvoy.Payload is IEnumerable<IStorageFile> )
				{
					IEnumerable<IStorageFile> ISFs = ( IEnumerable<IStorageFile> ) UsableConvoy.Payload;

					foreach ( IStorageFile ISF in ISFs )
					{
						string Content;
						if ( Enc == null )
						{
							Content = await ISF.ReadString();
						}
						else
						{
							Crawler.PLog( this, Res.SSTR( "ReadEncoding", Enc.EncodingName ), LogType.INFO );

							if ( !DecodeHtml )
							{
								await ISF.WriteString( await ISF.ReadString( Enc ) );
								continue;
							}

							Crawler.PLog( this, Res.SSTR( "ConvertEncoding", ISF.Name ), LogType.INFO );
							Content = await ISF.ReadString( Enc );
						}

						if ( DecodeHtml )
						{
							Content = WebUtility.HtmlDecode( Content );
						}

						await ISF.WriteString( Content );
					}

					return new ProcConvoy( this, UsableConvoy.Payload );
				}
				else
				{
					string Content = ( string ) UsableConvoy.Payload;
					if ( Enc != null )
					{
						Crawler.PLog( this, Res.RSTR( "CantConvertStringLiterals" ), LogType.INFO );
					}

					if ( DecodeHtml )
					{
						return new ProcConvoy( this, WebUtility.HtmlDecode( Content ) );
					}
				}
			}
			catch ( Exception ex )
			{
				Crawler.PLog( this, Res.SSTR( "EncodingFalied", ex.Message ), LogType.INFO );
			}

			return new ProcConvoy( this, null );
		}

		private bool DoNothing()
		{
			return ( CodePage == Encoding.UTF8.CodePage ) && !DecodeHtml;
		}

		public override void ReadParam( XParameter Param )
		{
			base.ReadParam( Param );

			XParameter[] RegParams = Param.Parameters( "i" );
			CodePage = Param.GetSaveInt( "CodePage" );
			DecodeHtml = Param.GetBool( "DecodeHtml" );
		}

		public override XParameter ToXParam()
		{
			XParameter Param = base.ToXParam();

			Param.SetValue( new XKey[] {
				new XKey( "CodePage", CodePage )
				, new XKey( "DecodeHtml", DecodeHtml )
			} );

			return Param;
		}
	}
}