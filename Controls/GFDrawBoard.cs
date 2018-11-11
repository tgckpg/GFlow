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
	using EventsArgs;

	class GFDrawBoard : IGFContainer
	{
		private CanvasControl Stage;

		private GFElement HitTarget;

		private Vector2 PrevDragPos;
		private IGFDraggable DragTarget;

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
					foreach ( GFElement x in Children )
					{
						x.CCRefresh.SetTarget( Refresh );
						x.Draw( ds, null, null );
						if ( x is IGFContainer GFC )
						{
							DrawR( ds, GFC );
						}
					}
				}
			}
		}

		public IList<T> FilterElements<T>()
			where T : GFElement
		{
			lock ( Children )
			{
				List<T> Pool = new List<T>();
				_FilterElement( this, Pool );
				return Pool;
			}
		}

		private void _FilterElement<T>( IGFContainer Container, List<T> Pool )
			where T : GFElement
		{
			Container.Children.AggExec( ( a, b, s ) =>
			{
				if ( s < 2 )
				{
					if ( b is T )
					{
						Pool.Add( ( T ) b );
					}

					if ( b is IGFContainer GFC )
					{
						_FilterElement( GFC, Pool );
					}
				}
			} );
		}

		private void Refresh( bool IntegrityCheck )
		{
			if ( IntegrityCheck )
			{
				// Get all synapses
				List<GFElement> DrawElements = new List<GFElement>();
				WalkElements( this, DrawElements, x => x is GraphElements.GFSynapse );

				// Link is broken if it contains synapse that is not drawn, remove them
				Children
					.OfType<GraphElements.GFLink>()
					.Where( x => !( DrawElements.Contains( x.From ) && DrawElements.Contains( x.To ) ) )
					.ToArray()
					.ExecEach( x => Children.Remove( x ) );
			}

			Stage.Invalidate();
		}

		private void DrawR( CanvasDrawingSession ds, IGFContainer Container )
		{
			GFElement GCElem = Container as GFElement;
			Container.Children.AggExec( ( a, b, s ) =>
			{
				if ( s < 2 )
				{
					b.CCRefresh.SetTarget( Refresh );
					b.Draw( ds, GCElem, a );
					if ( b is IGFContainer GFC )
					{
						DrawR( ds, GFC );
					}
				}
			} );
		}

		private void WalkElements( IGFContainer Container, List<GFElement> Elements, Func<GFElement, bool> Filter )
		{
			foreach ( GFElement b in Container.Children )
			{
				if ( Filter( b ) )
				{
					Elements.Add( b );
				}

				if ( b is IGFContainer GFC )
				{
					WalkElements( GFC, Elements, Filter );
				}
			}
		}

		private void Stage_PointerMoved( object sender, PointerRoutedEventArgs e )
		{
			Vector2 Pos = e.GetCurrentPoint( Stage ).Position.ToVector2();

			if ( DragTarget != null )
			{
				Vector2 Delta = Pos - PrevDragPos;
				PrevDragPos = Pos;
				DragTarget.Drag( Delta.X, Delta.Y, Pos.X, Pos.Y );
				Stage.Invalidate();
				return;
			}

			PrevDragPos = Pos;
			GFElement Hit = HitTests( Pos, this );

			if ( HitTarget != Hit )
			{
				if ( HitTarget is IGFDraggable Draggable )
				{
					Draggable.DragHandle.MouseOut?.Invoke( this, new GFPointerEventArgs() { Target = Draggable.DragHandle } );
				}
				else if ( HitTarget is IGFInteractive Btn )
				{
					Btn.MouseOut?.Invoke( this, new GFPointerEventArgs() { Target = HitTarget } );
				}

				HitTarget = Hit;
				Stage.Invalidate();
			}
		}

		private void Stage_PointerReleased( object sender, PointerRoutedEventArgs e )
		{
			if ( HitTarget is IGFInteractive Button )
			{
				Button.MouseRelease?.Invoke( this, new GFPointerEventArgs() { Target = HitTarget } );
			}

			if ( DragTarget != null )
			{
				DragTarget = null;
				Stage.Invalidate();
			}
		}

		private void Stage_PointerPressed( object sender, PointerRoutedEventArgs e )
		{
			PrevDragPos = e.GetCurrentPoint( Stage ).Position.ToVector2();

			if ( HitTarget is IGFDraggable Draggable )
			{
				DragTarget = Draggable;
			}

			if ( HitTarget is IGFInteractive Button )
			{
				Button.MousePress?.Invoke( this, new GFPointerEventArgs() { Target = HitTarget, Pos = PrevDragPos } );
			}
		}

		private GFElement HitTests( Vector2 P, IGFContainer GFC )
		{
			lock ( GFC.Children )
			{
				foreach ( GFElement Child in GFC.Children.Reverse() )
				{
					if ( Child is IGFDraggable Draggable && Draggable.DragHandle.HitTest( P ) )
					{
						Draggable.DragHandle.MouseOver?.Invoke( this, new GFPointerEventArgs() { Target = Draggable.DragHandle } );
						return Child;
					}

					if ( Child is IGFInteractive Btn && Btn.HitTest( P ) )
					{
						Btn.MouseOver?.Invoke( this, new GFPointerEventArgs() { Target = Child } );
						return Child;
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