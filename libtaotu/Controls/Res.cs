using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace libtaotu.Controls
{
    static class Res
    {
        private static StringResources stp;


        public static Func<string> SSTR( string key, string ColonAfter )
        {
            return () => RSTR( key ) + ": " + ColonAfter;
        }

        /// <summary>
        /// An alias ProcPanel.RSTR
        /// </summary>
        public static string RSTR( string key, params object[] args )
        {
            if ( stp == null )
            {
                stp = new StringResources( "/libtaotu/PanelMessage" );
                return key;
            }

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