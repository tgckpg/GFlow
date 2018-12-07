using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

using Net.Astropenguin.Linq;

namespace GFlow.Controls.GraphElements
{
	using EventsArgs;

	class GFLink : GFElement, IGFInteractive
	{
		public Color LineBrush { get; set; } = Colors.Black;

		public GFSynapse From => L is GFTransmitter ? L : R;
		public GFSynapse To => L is GFReceptor ? L : R;

		public Action<object, GFPointerEventArgs> MouseOver { get; set; }
		public Action<object, GFPointerEventArgs> MouseOut { get; set; }
		public Action<object, GFPointerEventArgs> MousePress { get; set; }
		public Action<object, GFPointerEventArgs> MouseRelease => null;

		private GFSynapse L;
		private GFSynapse R;

		private Vector2[] LinePoints;

		public GFLink( GFSynapse From, GFSynapse To )
			: base()
		{
			if ( From is GFReceptor )
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

		public bool IsBetween( GFSynapse L, GFSynapse R )
		{
			return ( L == this.L && R == this.R ) || ( L == this.R && R == this.L );
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			LinePoints = Draw( ds, L.SnapPoint, R.SnapPoint, LineBrush );
		}

		public static Vector2[] Draw( CanvasDrawingSession ds, Vector2 L, Vector2 R, Color LineBrush )
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

				return new Vector2[] { R, M0, M1, M2, M3, L };
			}
			else
			{
				float MX = 0.5f * ( L.X + R.X );
				Vector2 M0 = R.SetX( MX );
				Vector2 M1 = L.SetX( MX );

				ds.DrawLine( R, M0, LineBrush );
				ds.DrawLine( M0, M1, LineBrush );
				ds.DrawLine( M1, L, LineBrush );

				return new Vector2[] { R, M0, M1, L };
			}
		}

		public bool HitTest( Vector2 p )
		{
			return LinePoints.AggAny( ( P0, P1, s ) =>
			{
				if ( s == 1 )
				{
					Boundary B = new Boundary( P0, P1 );
					B.XY = B.XY - 5 * Vector2.One;
					B.WH = B.WH + 10 * Vector2.One;
					return B.Test( p );
				}

				return false;
			} );
		}

		public bool HitTest( float x, float y ) => HitTest( new Vector2( x, y ) );

		protected override void SetDefaults()
		{
			base.SetDefaults();
			LinePoints = new Vector2[ 0 ];
			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
			MousePress = _MousePress;
		}

		private void _MouseOver( object sender, GFPointerEventArgs e )
		{
			LineBrush = Colors.OrangeRed;
		}

		private void _MouseOut( object sender, GFPointerEventArgs e )
		{
			LineBrush = Colors.Black;
		}

		private void _MousePress( object sender, GFPointerEventArgs e )
		{
			( ( GFDrawBoard ) sender ).Remove( this );
			TriggerRedraw( false );
		}

	}
}