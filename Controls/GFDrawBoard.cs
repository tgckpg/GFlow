using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

using Net.Astropenguin.Linq;

namespace GFlow.Controls
{
	using BasicElements;
	using EventsArgs;

	[DataContract]
	class GFDrawBoard : IGFContainer, IDisposable
	{
		public Guid BoardId { get; private set; } = Guid.NewGuid();

		public IList<GFElement> Children { get; set; } = new List<GFElement>();

		[DataMember]
		public Vector2 PanOffset { get; set; } = Vector2.Zero;

		public GFProcedure StartProc => Find<GFProcedure>( 1 ).FirstOrDefault( x => x.IsStart );

		private CanvasControl Stage;
		private GFElement HitTarget;
		private IGFDraggable DragTarget;

		private Vector2 PrevDragPos;
		private bool IsPanning = false;

		public GFDrawBoard( CanvasControl Canvas )
		{
			SetStage( Canvas );
		}

		public GFDrawBoard( Guid BoardId, CanvasControl Canvas )
			: this( Canvas )
		{
			this.BoardId = BoardId;
		}

		public void SetStage( CanvasControl Canvas )
		{
			if ( Stage == null )
			{
				Stage = Canvas;

				Stage.Draw += Stage_Draw;
				Stage.PointerMoved += Stage_PointerMoved;
				Stage.PointerPressed += Stage_PointerPressed;
				Stage.PointerReleased += Stage_PointerReleased;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public void Dispose()
		{
			try
			{
				Stage.Draw -= Stage_Draw;
				Stage.PointerMoved -= Stage_PointerMoved;
				Stage.PointerPressed -= Stage_PointerPressed;
				Stage.PointerReleased -= Stage_PointerReleased;
				Stage = null;
			}
			catch ( Exception ) { };
		}

		public void Add( GFElement Elem )
		{
			lock ( Children )
			{
				Children.Add( Elem );
				Stage?.Invalidate();
			}
		}

		public void Remove( GFElement Elem )
		{
			lock ( Children )
			{
				Children.Remove( Elem );
				Stage?.Invalidate();
			}
		}

		private void Stage_Draw( CanvasControl sender, CanvasDrawEventArgs args )
		{
			lock ( Children )
			{
				using ( CanvasDrawingSession ds = args.DrawingSession )
				{
					ds.Transform = Matrix3x2.CreateTranslation( PanOffset );
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

		/// <summary>
		/// Find and returns the list of elements for the specified type.
		/// </summary>
		/// <typeparam name="T">Type of element</typeparam>
		/// <param name="Depth">Maximum depth to search. Default(0) is as deep as possible.</param>
		/// <returns></returns>
		public IList<T> Find<T>( int Depth = 0 )
			where T : GFElement
		{
			lock ( Children )
			{
				List<T> Pool = new List<T>();
				_Find( this, Pool, 0, Depth );
				return Pool;
			}
		}

		private void _Find<T>( IGFContainer Container, List<T> Pool, int CurrDepth, int MaxDepth )
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

					if ( b is IGFContainer GFC && ( CurrDepth < MaxDepth || MaxDepth == 0 ) )
					{
						_Find( GFC, Pool, CurrDepth + +1, MaxDepth );
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

			Stage?.Invalidate();
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

			if ( IsPanning || DragTarget != null )
			{
				Vector2 Delta = Pos - PrevDragPos;
				PrevDragPos = Pos;
				if ( IsPanning )
				{
					PanOffset += Delta;
				}
				else
				{
					Pos = Pos - PanOffset;
					DragTarget.Drag( Delta.X, Delta.Y, Pos.X, Pos.Y );
				}

				Stage.Invalidate();
				return;
			}

			PrevDragPos = Pos;
			GFElement Hit = HitTests( Pos - PanOffset, this );

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
			if ( IsPanning )
			{
				IsPanning = false;
				return;
			}

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
			PointerPoint PP = e.GetCurrentPoint( Stage );
			if ( e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && PP.Properties.IsRightButtonPressed )
			{
				return;
			}

			PrevDragPos = PP.Position.ToVector2();

			if ( HitTarget == null )
			{
				IsPanning = true;
				return;
			}

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

		[DataMember]
		private Guid SDataBoardId
		{
			get
			{
				if ( BoardId.Equals( Guid.Empty ) )
				{
					BoardId = Guid.NewGuid();
				}
				return BoardId;
			}

			set => BoardId = value;
		}

		[DataMember]
		private IList<GFElement> SDataChildren
		{
			get => Find<GFProcedure>().Cast<GFElement>().ToArray();
			set => Children = new List<GFElement>( value );
		}

		[DataMember]
		private IList<SDataGFProcRel> SDataLinks
		{
			get
			{
				List<SDataGFProcRel> Links = new List<SDataGFProcRel>();

				GFPathTracer Tracer = new GFPathTracer( this );
				foreach( GFProcedure Proc in Find<GFProcedure>() )
				{
					Links.Add( new SDataGFProcRel() { Source = Proc, Targets = Tracer.ProcsLinkFrom( Proc ) } );
				}

				return Links;
			}

			set
			{
				GFPathTracer Tracer = new GFPathTracer( this );
				foreach( SDataGFProcRel Rel in value )
				{
					GFProcedure GFrom = Rel.Source;
					IEnumerator<SDataGFProcTarget> Targets = Rel.Targets.GetEnumerator();
					Tracer.RestoreLinks( Rel.Source, Rel.Targets );
				}
			}
		}

	}
}