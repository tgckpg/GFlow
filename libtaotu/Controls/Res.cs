using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Loaders;

namespace libtaotu.Controls
{
    static class Res
    {
        private static StringResources stp;

        /// <summary>
        /// An alias ProcPanel.RSTR
        /// </summary>
        public static string RSTR( string key, params object[] args )
        {
            if( stp == null ) stp = new StringResources( "/libtaotu/PanelMessage" );
            string s = stp.Str( key );

            try { s = string.Format( s, args ); }
            catch( Exception ) { }

            if ( string.IsNullOrEmpty( s ) ) return key;

            return s;
        }
    }
}
