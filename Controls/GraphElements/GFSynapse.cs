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

		public Vector2 SnapPoint;

		protected bool DrawConnector = false;
		protected Vector2 EndPoint;

		protected IEnumerable<GFSynapse> TargetEndPoints;
		protected GFSynapse SnappedTarget;

		public GFSynapse()
		{
			ActualBounds.W = 20;
			MouseOver = _MouseOver;
			MouseOut = _MouseOut;

			MousePress = StartDrag;
			MouseRelease = EndDrag;
		}

		private void StartDrag( object sender, GFPointerEventArgs e )
		{
			EndPoint = e.Pos;
			DrawConnector = true;

			if ( this is GFSynapseL )
			{
				TargetEndPoints = ( ( GFDrawBoard ) sender ).FilterElements<GFSynapseR>().Cast<GFSynapse>();
			}
			else if ( this is GFSynapseR )
			{
				TargetEndPoints = ( ( GFDrawBoard ) sender ).FilterElements<GFSynapseL>().Cast<GFSynapse>();
			}
		}

		private void EndDrag( object sender, GFPointerEventArgs e )
		{
			DrawConnector = false;
			TargetEndPoints = null;

			if( SnappedTarget != null )
			{
				( ( GFDrawBoard ) sender ).Children.Add( new GFLink( this, SnappedTarget ) );
			}
		}

		private static void _MouseOver( object sender, GFPointerEventArgs e )
		{
			( ( GFSynapse ) e.Target ).SNFill = Colors.Cyan;
		}

		private static void _MouseOut( object sender, GFPointerEventArgs e )
		{
			( ( GFSynapse ) e.Target ).SNFill = Colors.Black;
		}

		public void Drag( float x, float y, float ax, float ay )
		{
			if ( SnappedTarget?.ActualBounds.Test( ax, ay ) != true )
			{
				EndPoint.X = ax;
				EndPoint.Y = ay;

				SnappedTarget = TargetEndPoints.FirstOrDefault( b => b.ActualBounds.Test( EndPoint ) );
				if ( SnappedTarget != null )
				{
					EndPoint = SnappedTarget.SnapPoint;
				}
			}
		}
	}

	class GFSynapseL : GFSynapse
	{
		// This assumes Parent is always present
		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			ActualBounds.YH = Parent.ActualBounds.YH;
			ActualBounds.X = Parent.ActualBounds.X - 20;

			Vector2 M0 = new Vector2(
				ActualBounds.X + 15
				, Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H
			);

			ds.DrawLine( M0, M0.MoveX( 5 ), SNFill );
			ds.DrawLine( M0, M0.Move( -5 ), SNFill );
			ds.DrawLine( M0, M0.Move( -5, 5 ), SNFill );

			SnapPoint = M0.MoveX( -5 );

			if ( DrawConnector )
			{
				if ( SnappedTarget == null )
				{
					GFLink.Draw( ds, SnapPoint, EndPoint, SNFill );
				}
				else
				{
					GFLink.Draw( ds, SnapPoint, SnappedTarget.SnapPoint, SNFill );
				}
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

			Vector2 M0 = new Vector2(
				Parent.ActualBounds.XWs + 5
				, Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H
			);

			ds.DrawLine( M0, M0.MoveX( -5 ), SNFill );

			SnapPoint = M0.MoveX( 5 );

			ds.DrawLine( M0.MoveY( 5 ), SnapPoint, SNFill );
			ds.DrawLine( M0.MoveY( -5 ), SnapPoint, SNFill );

			if ( DrawConnector )
			{
				if ( SnappedTarget == null )
				{
					GFLink.Draw( ds, EndPoint, SnapPoint, SNFill );
				}
				else
				{
					GFLink.Draw( ds, SnappedTarget.SnapPoint, SnapPoint, SNFill );
				}
			}
		}
	}

}