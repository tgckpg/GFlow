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

	public enum SynapseType : byte { TRUNK, BRANCH }

	class GFSynapse : GFButton, IGFDraggable
	{
		public object Nucleus { get; protected set; }
		public object Dendrite00 { get; set; }

		public Color SNFill { get; set; } = Colors.Black;
		public GFButton DragHandle => this;

		public SynapseType SynapseType = SynapseType.TRUNK;

		public Vector2 SnapPoint;

		protected bool DrawConnector = false;
		protected Vector2 EndPoint;

		protected IEnumerable<GFSynapse> TargetEndPoints;
		protected GFSynapse SnappedTarget;

		public GFSynapse( object Nucleus )
		{
			this.Nucleus = Nucleus;

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

			if ( this is GFReceptor )
			{
				TargetEndPoints = ( ( GFDrawBoard ) sender ).Find<GFTransmitter>().Cast<GFSynapse>();
			}
			else if ( this is GFTransmitter )
			{
				TargetEndPoints = ( ( GFDrawBoard ) sender ).Find<GFReceptor>().Cast<GFSynapse>();
			}
		}

		private void EndDrag( object sender, GFPointerEventArgs e )
		{
			try
			{
				DrawConnector = false;
				TargetEndPoints = null;

				if ( SnappedTarget != null )
				{
					GFDrawBoard DrawBoard = ( GFDrawBoard ) sender;

					// NOTE: GFLink should only appear at the top level
					if ( DrawBoard.Find<GFLink>( 1 ).Any( x => x.IsBetween( this, SnappedTarget ) ) )
					{
						return;
					}

					DrawBoard.Children.Add( new GFLink( this, SnappedTarget ) );
				}
			}
			finally
			{
				EndPoint = Vector2.Zero;
				SnappedTarget = null;
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

	// This is the receiving end ">-"
	class GFReceptor : GFSynapse
	{
		public GFReceptor( object Nucleus ) : base( Nucleus ) { }

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

	// This is the giving end "->"
	class GFTransmitter : GFSynapse
	{
		public GFTransmitter( object Nucleus ) : base( Nucleus ) { }

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