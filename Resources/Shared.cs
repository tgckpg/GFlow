using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Loaders;

namespace GFlow.Resources
{
	using Models.Procedure;
	class Shared
	{
		public static Type SourceView;
		public static Type RenameDialog;

		public static Func<Uri, HttpRequest> CreateRequest = x => new HttpRequest( x ) { EN_UITHREAD = false };
	}
}