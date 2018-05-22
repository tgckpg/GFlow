using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

using Net.Astropenguin.Linq;

namespace GFlow.Controls.BasicElements
{
	class GFButton : IGFElement, IForeground, IBackground
	{
		public Vector2 P { get; set; }

		public Boundary Bounds { get; set; } = new Boundary() { W = 200, H = 24 };

		public Color FGFill { get; set; } = Colors.Black;
		public Color BGFill { get; set; } = Colors.Gray;

		public string Label { get; set; } = "Text Label";

		virtual public void Draw( CanvasDrawingSession ds )
		{
			ds.DrawRectangle( P.X + Bounds.X, P.Y + Bounds.Y, Bounds.W, Bounds.H, BGFill );
			ds.DrawText( Label, P, FGFill );
		}
	}

	class GFPanel : GFButton
	{
		IList<IGFElement> Children { get; set; }

		public override void Draw( CanvasDrawingSession ds )
		{
			base.Draw( ds );
			IGFElement Prev;
			Children.AggExec( ( C1, C2, PState ) =>
			{

				if( PState == 1 )
				{
					C2.P.Y = C1.P.Y + C1.
				}

				if ( PState < 2 )
				{
					C2.Draw( ds );
				}

			} );

		}
	}

}
