using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

using Net.Astropenguin.Linq;

namespace GFlow.Controls.BasicElements
{
	using EventsArgs;

	class GFButton : GFElement, IForeground, IBackground, IGFInteractive
	{
		public Color FgFill { get; set; } = Colors.Black;
		public Color BgFill { get; set; } = Color.FromArgb( 0xFF, 0xA0, 0xA0, 0xA0 );

		public Boundary Padding { get; set; } = new Boundary( 5, 10 );

		public Action<object, GFPointerEventArgs> MouseOver { get; set; }
		public Action<object, GFPointerEventArgs> MouseOut { get; set; }

		public Action<object, GFPointerEventArgs> MousePress { get; set; }
		public Action<object, GFPointerEventArgs> MouseRelease { get; set; }

		public HorizontalAlignment HAlign { get; set; } = HorizontalAlignment.Left;

		public GFButton()
		{
			Bounds.W = 200;
			Bounds.H = 24;
		}

		public bool HitTest( Vector2 p ) => ActualBounds.Test( p );
		public bool HitTest( float x, float y ) => ActualBounds.Test( x, y );

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			Vector2 pXY = Vector2.Zero;
			if ( Parent != null )
			{
				pXY = Parent.Bounds.XY;
			}

			// Default is Left
			ActualBounds.X = DrawOffset.X + pXY.X + Bounds.X;
			ActualBounds.W = Padding.LRs + Bounds.W;

			if ( !( Parent == null || Prev == null ) )
			{
				if ( HAlign == HorizontalAlignment.Right )
				{
					ActualBounds.X += Prev.ActualBounds.W - Bounds.W - Padding.LRs;
				}
				else if ( HAlign == HorizontalAlignment.Stretch )
				{
					ActualBounds.W = Prev.ActualBounds.W;
				}
			}

			ActualBounds.Y = DrawOffset.Y + pXY.Y + Bounds.Y;
			ActualBounds.H = Padding.TBs + Bounds.H;

			ds.FillRectangle( ActualBounds.X, ActualBounds.Y, ActualBounds.W, ActualBounds.H, BgFill );
		}
	}

	interface IGFDraggable
	{
		GFButton DragHandle { get; }
		void Drag( float x, float y, float ax, float ay );
	}

	class GFPanel : GFElement, IGFContainer
	{
		public IList<GFElement> Children { get; set; } = new List<GFElement>();

		public Orientation Orientation { get; set; } = Orientation.Vertical;

		public GFPanel()
		{
		}

		public void Add( GFElement Elem )
		{
			lock ( Children ) Children.Add( Elem );
		}

		public void Remove( GFElement Elem )
		{
			lock ( Children ) Children.Remove( Elem );
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			if ( Orientation == Orientation.Vertical )
			{
				DrawV( ds, Parent, Prev );
			}
			else
			{
				DrawH( ds, Parent, Prev );
			}
		}

		private void DrawH( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			float MovBound = 0;
			float MaxBound = 0;

			if ( Prev != null )
			{
				Bounds.X = Prev.ActualBounds.X;
				Bounds.Y = Prev.ActualBounds.YHs;
				MaxBound = Prev.ActualBounds.H;
			}
			else if ( Parent != null )
			{
				Bounds.XY = Parent.Bounds.XY;
			}

			lock ( Children )
			{
				Children.AggExec( ( C1, C2, PState ) =>
				{
					if ( PState < 2 )
					{
						C2.DrawOffset = new Vector2( MovBound, 0 );
						C2.Draw( ds, this, C1 );
						MovBound += C2.ActualBounds.W;
						MaxBound = Math.Max( MaxBound, C2.ActualBounds.H );
					}
				} );
			}

			ActualBounds.XY = Bounds.XY;
			ActualBounds.W = MovBound;
			ActualBounds.H = MaxBound;
		}

		private void DrawV( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			float MovBound = 0;
			float MaxBound = 0;

			if ( Prev != null )
			{
				Bounds.X = Prev.ActualBounds.X;
				Bounds.Y = Prev.ActualBounds.YHs;
				MaxBound = Prev.ActualBounds.W;
			}
			else if ( Parent != null )
			{
				Bounds.XY = Parent.Bounds.XY;
			}

			lock ( Children )
			{
				Children.AggExec( ( C1, C2, PState ) =>
				{
					if ( PState < 2 )
					{
						C2.DrawOffset = new Vector2( 0, MovBound );
						C2.Draw( ds, this, C1 );
						MovBound += C2.ActualBounds.H;
						MaxBound = Math.Max( MaxBound, C2.ActualBounds.W );
					}
				} );
			}

			ActualBounds.XY = Bounds.XY;
			ActualBounds.W = MaxBound;
			ActualBounds.H = MovBound;
		}

	}
}