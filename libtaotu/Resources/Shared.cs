using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Loaders;

namespace libtaotu.Resources
{
	using Models.Procedure;
	class Shared
	{
		public static Type SourceView;
		public static Type RenameDialog;

		private static Dictionary<ProcType, Type> ExProcs = new Dictionary<ProcType, Type>();

		public static void AddProcType( ProcType P, Type ProcType )
		{
			if ( !ProcType.GetTypeInfo().IsSubclassOf( typeof( Procedure ) ) )
				throw new ArgumentException( "ProcType" );

			ExProcs[ P ] = ProcType;
		}

		public static Procedure ProcCreate( ProcType P )
		{
			return ( Procedure ) Activator.CreateInstance( ExProcs[ P ] );
		}

		public static Func<Uri, HttpRequest> CreateRequest = x => new HttpRequest( x ) { EN_UITHREAD = false };
	}
}