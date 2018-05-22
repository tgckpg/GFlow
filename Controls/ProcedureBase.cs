using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls
{
	using Models.Procedure;

	class GFProcedure : IGFPanel, IGFDraggable
	{
		public IGFConnector<GFProcedure> Input { get; set; }
		public IGFConnector<GFProcedure> Output { get; set; }

		public List<IGFProperty<GFProcedure>> SubProcs { get; set; } = new List<IGFProperty<GFProcedure>>();

		public Procedure Properties { get; set; }
		public Vector4 Bounds { get; set; }

		public float X { get; set; }
		public float Y { get; set; }

		public Boundary DragHandle { get; set; }

		public void Drag( float dx, float dy )
		{
			X += dx;
			Y += dy;
		}

	}
}