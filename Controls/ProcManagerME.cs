using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;

namespace GFlow.Controls
{
	partial class ProcManager
	{
		public static ProcManager Load( Stream s ) => ReadGF( s ) ?? ReadLegacy( s );

		private static ProcManager ReadLegacy( Stream s )
		{
			try
			{
				s.Seek( 0, SeekOrigin.Begin );
				StreamReader Reader = new StreamReader( s, Encoding.UTF8 );

				XRegistry XReg = new XRegistry( Reader.ReadToEnd(), null, false );
				XParameter Param = XReg.Parameter( "Procedures" );
				return new ProcManager( Param );
			}
			catch ( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.WARNING );
			}

			return null;
		}

		private static ProcManager ReadGF( Stream s )
		{
			try
			{
				s.Seek( 0, SeekOrigin.Begin );
				DataContractSerializer DCS = new DataContractSerializer( typeof( GFDrawBoard ) );
				GFDrawBoard DBoard = DCS.ReadObject( s ) as GFDrawBoard;
				GFPathTracer Tracer = new GFPathTracer( DBoard );
				ProcManager PM = Tracer.CreateProcManager( DBoard.StartProc );
				PM.GUID = DBoard.BoardId.ToString();
				return PM;
			}
			catch ( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.WARNING );
			}

			return null;
		}
	}
}