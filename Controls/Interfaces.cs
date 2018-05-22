using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls
{
	class Boundary
	{
		public float X, Y, W, H;

		public float Top { get => X; set => X = value; }
		public float Left { get => Y; set => Y = value; }
		public float Right { get => W; set => W = value; }
		public float Bottom { get => H; set => H = value; }

		public float LeftRight { get => Left; set { Left = value; Right = value; } }
		public float TopBottom { get => Top; set { Top = value; Bottom = value; } }

		public Boundary() { }

		public Boundary( float X, float Y, float W, float H )
		{
			this.X = X;
			this.Y = Y;
			this.W = W;
			this.H = H;
		}

		public Boundary( float Padding ) => X = Y = W = H = Padding;

		public Boundary( float TopBottom, float LeftRight )
		{
			Top = Bottom = TopBottom;
			Left = Right = LeftRight;
		}

		public Vector2 XY => new Vector2( X, Y );
		public Vector2 WH => new Vector2( W, H );
		public Vector4 XYWH => new Vector4( X, Y, W, H );

		public bool Test( Vector2 p )
		{
			return ( X <= p.X && p.X <= ( X + W ) ) && ( Y <= p.Y && p.Y <= ( Y + H ) );
		}
	}

	abstract class GFElement
	{
		virtual public Boundary Bounds { get; set; } = new Boundary();
		virtual public Boundary ActualBounds { get; internal set; }

		public Vector2 DrawOffset { get; set; }

		abstract public void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev );
	}

	interface IBorder
	{
		Color BDFill { get; set; }
		float BDThinkness { get; set; }
	}

	interface IForeground { Color FGFill { get; set; } }
	interface IBackground { Color BGFill { get; set; } }

	interface IGFContainer
	{
		IList<GFElement> Children { get; set; }
		void Add( GFElement e );
		void Remove( GFElement e );

	}

	interface IGFProperty<T>
	{
		string Key { get; set; }
		string Name { get; set; }

		bool InReceptor { get; set; }
		bool OutConnector { get; set; }

		IGFConnector<T> Receptor { get; set; }
		IGFConnector<T> Connector { get; set; }
	}

	interface IGFConnector<T>
	{
		T Target { get; set; }
	}

}