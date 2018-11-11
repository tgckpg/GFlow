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
		private static Dictionary<string, (Type, string)> Registered = new Dictionary<string, (Type, string)>()
		{
			{ "URLLIST", ( typeof( ProcUrlList ), "/GFlow/ProcItems:URLLIST" ) }
			, { "GENERATOR", ( typeof( ProcGenerator ), "/GFlow/ProcItems:GENERATOR" ) }
			, { "FIND", ( typeof( ProcFind ), "/GFlow/ProcItems:FIND" ) }
			, { "PARAMETER", ( typeof( ProcParameter ), "/GFlow/ProcItems:PARAMETER" ) }
			, { "CHKRA", ( typeof( ProcChakra ), "/GFlow/ProcItems:CHAKRA" ) }
			, { "ENCODING", ( typeof( ProcEncoding ), "/GFlow/ProcItems:ENCODING" ) }
			, { "RESULT", ( typeof( ProcResult ), "/GFlow/ProcItems:RESULT" ) }
		};

		public static string Register( string ProcKey, string NameResPath, Type TInfo )
		{
			Registered[ ProcKey ] = (TInfo, NameResPath);
			return ProcKey;
		}

		public static Procedure Create( string Name )
		{
			return ( Procedure ) Activator.CreateInstance( Registered[ Name ].Item1 );
		}

		public Dictionary<string, string> ProcChoices { get; set; } = new Dictionary<string, string>();

		public GFProcedureList()
		{
			foreach ( KeyValuePair<string, (Type, string)> KV in Registered )
			{
				string[] Str = KV.Value.Item2.Split( ':' );
				StringResources stx = StringResources.Load( Str[ 0 ] );
				ProcChoices[ KV.Key ] = stx.Str( Str[ 1 ] );
			}
		}

	}
}