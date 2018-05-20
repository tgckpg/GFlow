using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls
{
	using Models.Procedure;

	class GFProcedure : IGFButton, IGFDraggable
	{
		public IGFConnector<GFProcedure> Input { get; set; }
		public IGFConnector<GFProcedure> Output { get; set; }

		public List<IGFProperty<GFProcedure>> SubProcs { get; set; } = new List<IGFProperty<GFProcedure>>();

		public Procedure Properties { get; set; }
	}
}
}