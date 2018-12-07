using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace GFlow.Controls
{
	static class Res
	{
		private static StringResources stp = StringResources.Load( "/GFlow/PanelMessage" );

		public static string SSTR( string key, string ColonAfter )
		{
			return RSTR( key ) + ": " + ColonAfter;
		}

		public static string RSTR( string key, params object[] args )
		{
			try
			{
				string s = string.Format( stp.Str( key ), args );
				return s;
			}
			catch ( Exception ex )
			{
				Logger.Log( "RSTR", ex.Message, LogType.WARNING );
			}

			return key;
		}
	}
}