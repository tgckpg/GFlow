using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls.GraphElements
{
	class GFNode : GFTextButton, IGFContainer
	{
		public IList<GFElement> Children { get; set; }

		public GFNode()
			: base()
		{
			Children = new List<GFElement>();
		}

		public void Add( GFElement e ) => Children.Add( e );
		public void Remove( GFElement e ) => Children.Remove( e );
	}
}