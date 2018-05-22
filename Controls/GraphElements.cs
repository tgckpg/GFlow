using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls.GraphElements
{
	using BasicElements;
	using Microsoft.Graphics.Canvas;

	class GFNode : GFButton
	{
		public bool SynapseL { get; set; }
		public bool SynapseR { get; set; }

		public Color SNFill { get; set; } = Colors.Black;

		public GFNode()
			: base()
		{
			BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );

			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
		}

		private static void _MouseOver( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xD0, 0xD0, 0xD0, 0xD0 );
		}

		private static void _MouseOut( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			base.Draw( ds, Parent, Prev );

			float HH = ActualBounds.Y + 0.5f * ActualBounds.H;

			if ( SynapseL )
			{
				float MX = ActualBounds.X - 10.0f;
				ds.DrawLine( ActualBounds.X, HH, MX, HH, SNFill );
				ds.DrawLine( MX, HH, MX - 5, HH - 5, SNFill );
				ds.DrawLine( MX, HH, MX - 5, HH + 5, SNFill );
			}

			if ( SynapseR )
			{
				float MX = ActualBounds.X + ActualBounds.W + 10.0f;
				ds.DrawLine( MX - 10.0f, HH, MX, HH, SNFill );
				ds.DrawLine( MX, HH, MX + 5, HH - 5, SNFill );
				ds.DrawLine( MX, HH, MX + 5, HH + 5, SNFill );
			}

		}
	}
}