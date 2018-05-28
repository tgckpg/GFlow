using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

using Net.Astropenguin.Linq;

namespace GFlow.Controls.BasicElements
{
	using EventsArgs;
	class GFButton : GFElement, IForeground, IBackground
	{
		public Color FGFill { get; set; } = Colors.Black;
		public Color BGFill { get; set; } = Color.FromArgb( 0xFF, 0xA0, 0xA0, 0xA0 );

		public Boundary Padding { get; set; } = new Boundary( 5, 10 );

		public CanvasTextFormat LabelFormat { get; set; } = new CanvasTextFormat() { FontSize = 18 };
		public string Label { get; set; } = "Text Label";

		public Action<object, GFPointerEventArgs> MouseOver { get; set; }
		public Action<object, GFPointerEventArgs> MouseOut { get; set; }

		public Action<object, GFPointerEventArgs> MousePress { get; set; }
		public Action<object, GFPointerEventArgs> MouseRelease { get; set; }

		public GFButton()
		{
			Bounds.W = 200;
			Bounds.H = 24;
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			Vector2 pXY = Vector2.Zero;
			if ( Parent != null )
			{
				pXY = Parent.Bounds.XY;
			}

			CanvasTextLayout TL = new CanvasTextLayout( ds, Label, LabelFormat, Bounds.W, Bounds.H );

			ActualBounds.X = DrawOffset.X + pXY.X + Bounds.X;
			ActualBounds.Y = DrawOffset.Y + pXY.Y + Bounds.Y;
			ActualBounds.W = Padding.LRs + Bounds.W;
			ActualBounds.H = Padding.TBs + Bounds.H;

			ds.FillRectangle( ActualBounds.X, ActualBounds.Y, ActualBounds.W, ActualBounds.H, BGFill );
			ds.DrawTextLayout( TL, ActualBounds.X + Padding.Left, ActualBounds.Y + Padding.Top, FGFill );
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
			Vector2 VH = Vector2.Zero;

			if ( Prev != null )
			{
				VH.Y = Prev.ActualBounds.H;
			}

			lock ( Children )
			{
				Children.AggExec( ( C1, C2, PState ) =>
				{
					if ( PState < 2 )
					{
						C2.DrawOffset = VH;
						C2.Draw( ds, this, C1 );

						VH.Y += C2.ActualBounds.H;
					}
				} );
			}

			ActualBounds = new Boundary( Bounds.X, Bounds.Y, Bounds.W, VH.Y );
		}
	}

}