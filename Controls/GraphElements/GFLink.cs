using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls.GraphElements
{
	class GFLink : GFElement
	{
		public Color LineBrush { get; set; } = Colors.Black;

		private GFSynapse L;
		private GFSynapse R;

		public GFLink( GFSynapse From, GFSynapse To )
		{
			if ( From is GFSynapseL )
			{
				L = From;
				R = To;
			}
			else
			{
				L = To;
				R = From;
			}
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			Draw( ds, L.SnapPoint, R.SnapPoint, LineBrush );
		}

		public static void Draw( CanvasDrawingSession ds, Vector2 L, Vector2 R, Color LineBrush )
		{
			if ( L.X < R.X )
			{
				float MY = 0.5f * ( L.Y + R.Y );

				Vector2 M0 = R.MoveX( 20 );
				Vector2 M1 = M0.SetY( MY );
				Vector2 M3 = L.MoveX( -20 );
				Vector2 M2 = M3.SetY( MY );

				ds.DrawLine( R, M0, LineBrush );
				ds.DrawLine( M0, M1, LineBrush );
				ds.DrawLine( M1, M2, LineBrush );
				ds.DrawLine( M2, M3, LineBrush );
				ds.DrawLine( M3, L, LineBrush );
			}
			else
			{
				float MX = 0.5f * ( L.X + R.X );
				Vector2 M0 = R.SetX( MX );
				Vector2 M1 = L.SetX( MX );

				ds.DrawLine( R, M0, LineBrush );
				ds.DrawLine( M0, M1, LineBrush );
				ds.DrawLine( M1, L, LineBrush );
			}
		}

	}
}