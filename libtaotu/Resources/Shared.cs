using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using libtaotu.Models.Procedure;
using System.Reflection;

namespace libtaotu.Resources
{
    class Shared
    {
        public static Type SourceView;

        public static Type ProcExtractor { get; private set; }
        public static Type ProcMarker { get; private set; }

        public static void SetExtractor( Type T )
        {
            if( T.GetTypeInfo().IsSubclassOf( typeof( Procedure ) ) )
            {
                ProcExtractor = T;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public static void SetMarker( Type T )
        {
            if( T.GetTypeInfo().IsSubclassOf( typeof( Procedure ) ) )
            {
                ProcMarker = T;
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }
}
