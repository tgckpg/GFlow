using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls.GraphElements
{
	using BasicElements;
	using EventsArgs;

	class GFSynapse : GFButton, IGFDraggable
	{
		public Color SNFill { get; set; } = Colors.Black;
		public GFButton DragHandle => this;

		protected bool DrawConnector = false;
		protected Vector2 EndPoint;

		public GFSynapse()
		{
			ActualBounds.W = 20;
			MouseOver = _MouseOver;
			MouseOut = _MouseOut;

			MousePress = _StartDrag;
			MouseRelease = _EndDrag;
		}

		private void _StartDrag( object sender, GFPointerEventArgs e )
		{
			EndPoint = e.Pos;
			DrawConnector = true;
		}

		private void _EndDrag( object sender, GFPointerEventArgs e )
		{
			DrawConnector = false;
		}

		private static void _MouseOver( object sender, GFPointerEventArgs e )
		{
			( ( GFSynapse ) e.Target ).SNFill = Colors.Cyan;
		}

		private static void _MouseOut( object sender, GFPointerEventArgs e )
		{
			( ( GFSynapse ) e.Target ).SNFill = Colors.Black;
		}

		public void Drag( float x, float y )
		{
			EndPoint.X += x;
			EndPoint.Y += y;
		}
	}

	class GFSynapseL : GFSynapse
	{
		// This assumes Parent is always present
		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			ActualBounds.YH = Parent.ActualBounds.YH;
			ActualBounds.X = Parent.ActualBounds.X - 20;

			float HH = Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H;
			float MX = ActualBounds.X + 15;

			ds.DrawLine( MX, HH, MX + 5, HH, SNFill );
			ds.DrawLine( MX, HH, MX - 5, HH - 5, SNFill );
			ds.DrawLine( MX, HH, MX - 5, HH + 5, SNFill );

			if ( DrawConnector )
			{
				MX -= 5;

				float S = ( MX < ( EndPoint.X + 20 ) )
					? MX - 20
					: 0.5f * ( MX + EndPoint.X );

				ds.DrawLine( MX, HH, S, HH, SNFill );
				ds.DrawLine( S, HH, S, EndPoint.Y, SNFill );
				ds.DrawLine( S, EndPoint.Y, EndPoint.X, EndPoint.Y, SNFill );
			}
		}
	}

	class GFSynapseR : GFSynapse
	{
		// This assumes Parent is always present
		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			ActualBounds.YH = Parent.ActualBounds.YH;
			ActualBounds.X = Parent.ActualBounds.XWs;

			float HH = Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H;
			float MX = Parent.ActualBounds.XWs + 5;

			ds.DrawLine( MX, HH, MX - 5, HH, SNFill );
			ds.DrawLine( MX, HH, MX + 5, HH - 5, SNFill );
			ds.DrawLine( MX, HH, MX + 5, HH + 5, SNFill );

			if ( DrawConnector )
			{
				MX += 5;

				float S = ( ( EndPoint.X - 20 ) < MX )
					? MX + 20
					: 0.5f * ( MX + EndPoint.X );

				ds.DrawLine( MX, HH, S, HH, SNFill );
				ds.DrawLine( S, HH, S, EndPoint.Y, SNFill );
				ds.DrawLine( S, EndPoint.Y, EndPoint.X, EndPoint.Y, SNFill );
			}
		}
	}
}