using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libtaotu.Models.Procedure
{
	using Resources;
	internal class ProcMark
	{
		public static Procedure Create()
		{
			if( Shared.ProcMarker == null )
			{
				throw new InvalidOperationException( "Please define the Shared Marker Class" );
			}

			return ( Procedure ) Activator.CreateInstance( Shared.ProcMarker );
		}
	}
}