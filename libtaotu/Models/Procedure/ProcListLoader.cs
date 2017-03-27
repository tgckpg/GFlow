using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libtaotu.Models.Procedure
{
	using Resources;
	internal class ProcListLoader
	{
		public static Procedure Create()
		{
			if( Shared.ProcListLoader == null )
			{
				throw new InvalidOperationException( "Please define the Shared ListLoader Class" );
			}

			return ( Procedure ) Activator.CreateInstance( Shared.ProcListLoader );
		}
	}
}