using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls.EventsArgs
{
	class GFPointerEventArgs
	{
		public Vector2 Pos { get; set; }
		public GFElement Target { get; set; }
	}
}