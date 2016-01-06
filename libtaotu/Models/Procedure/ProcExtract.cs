using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace libtaotu.Models.Procedure
{
    using Resources;

    internal class ProcExtract
    {
        public static Procedure Create()
        {
            if( Shared.ProcExtractor == null )
            {
                throw new InvalidOperationException( "Please define the Shared Extractor Class" );
            }

            return ( Procedure ) Activator.CreateInstance( Shared.ProcExtractor );
        }
    }
}
