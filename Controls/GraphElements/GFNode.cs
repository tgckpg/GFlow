using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls.GraphElements
{
	using BasicElements;
	using EventsArgs;

	class GFNode : GFButton, IGFContainer
	{
		// For hit tests
		public IList<GFElement> Children { get; set; }

		public GFNode()
			: base()
		{
			BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
			Children = new List<GFElement>();

			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
		}

		private static void _MouseOver( object sender, GFPointerEventArgs e )
		{
			( ( GFNode ) e.Target ).BGFill = Color.FromArgb( 0xFF, 0xD0, 0xD0, 0xD0 );
		}

		private static void _MouseOut( object sender, GFPointerEventArgs e )
		{
			( ( GFNode ) e.Target ).BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
		}

		public void Add( GFElement e ) => throw new NotSupportedException();
		public void Remove( GFElement e ) => throw new NotSupportedException();
	}
}