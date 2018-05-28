using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Loaders;

namespace GFlow.Controls
{
	using Models.Procedure;

	class GFProcedureList
	{
		public Dictionary<ProcType, string> ProcChoices { get; set; } = new Dictionary<ProcType, string>();

		public GFProcedureList()
		{
			StringResources stx = StringResources.Load( "/GFlow/ProcItems" );

			Type PType = typeof( ProcType );
			foreach ( ProcType P in Enum.GetValues( PType ) )
			{
				string ProcName = stx.Str( Enum.GetName( PType, P ) );

				if ( string.IsNullOrEmpty( ProcName ) ) continue;

				ProcChoices.Add( P, ProcName );
			}
		}

	}
}