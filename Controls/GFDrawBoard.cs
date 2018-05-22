using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

using Net.Astropenguin.Linq;

namespace GFlow.Controls
{
	using BasicElements;

	class GFDrawBoard : IGFContainer
	{
		private CanvasControl Stage;

		public IList<GFElement> Children { get; set; } = new List<GFElement>();

		public GFDrawBoard( CanvasControl Canvas )
		{
			Stage = Canvas;

			Stage.Draw += Stage_Draw;
			Stage.PointerMoved += Stage_PointerMoved;
			Stage.PointerPressed += Stage_PointerPressed;
			Stage.PointerReleased += Stage_PointerReleased;
		}

		public void Add( GFElement Elem )
		{
			lock ( Children )
			{
				Children.Add( Elem );
				Stage.Invalidate();
			}
		}

		public void Remove( GFElement Elem )
		{
			lock ( Children )
			{
				Children.Remove( Elem );
				Stage.Invalidate();
			}
		}

		private void Stage_Draw( CanvasControl sender, CanvasDrawEventArgs args )
		{
			lock ( Children )
			{
				using ( CanvasDrawingSession ds = args.DrawingSession )
				{
					foreach ( GFElement Child in Children )
					{
						Child.Draw( ds, null, null );
					}
				}
			}
		}

		private GFElement HitTarget;

		private Vector2 PrevDragPos;
		private IGFDraggable DragTarget;

		private void Stage_PointerMoved( object sender, PointerRoutedEventArgs e )
		{
			Vector2 Pos = e.GetCurrentPoint( Stage ).Position.ToVector2();

			if ( DragTarget != null )
			{
				Vector2 Delta = Pos - PrevDragPos;
				PrevDragPos = Pos;
				DragTarget.Drag( Delta.X, Delta.Y );
				Stage.Invalidate();
				return;
			}

			GFElement Hit = HitTests( Pos, this );

			if ( HitTarget != Hit )
			{
				if ( HitTarget is IGFDraggable Draggable )
				{
					Draggable.DragHandle.MouseOut?.Invoke( Draggable.DragHandle );
				}
				else if ( HitTarget is GFButton Btn )
				{
					Btn.MouseOut?.Invoke( Btn );
				}

				HitTarget = Hit;
				Stage.Invalidate();
			}
		}

		private void Stage_PointerReleased( object sender, PointerRoutedEventArgs e )
		{
			if( DragTarget != null )
			{
				DragTarget = null;
			}
		}

		private void Stage_PointerPressed( object sender, PointerRoutedEventArgs e )
		{
			if( HitTarget is IGFDraggable Draggable )
			{
				PrevDragPos = e.GetCurrentPoint( Stage ).Position.ToVector2();
				DragTarget = Draggable;
			}
		}

		private GFElement HitTests( Vector2 P, IGFContainer GFC )
		{
			lock ( GFC.Children )
			{
				foreach ( GFElement Child in GFC.Children )
				{
					if( Child is IGFDraggable Draggable && Draggable.DragHandle.ActualBounds.Test( P ) )
					{
						Draggable.DragHandle.MouseOver?.Invoke( Draggable.DragHandle );
						return Child;
					}

					if ( Child is GFButton Btn && Btn.ActualBounds.Test( P ) )
					{
						Btn.MouseOver?.Invoke( Btn );
						return Btn;
					}

					if ( Child is IGFContainer GFC2 )
					{
						GFElement KHit = HitTests( P, GFC2 );
						if ( KHit != null )
						{
							return KHit;
						}
					}
				}
			}

			return null;
		}

	}
}