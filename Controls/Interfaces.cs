using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls
{
	using EventsArgs;

	[DataContract]
	class Boundary
	{
		[DataMember]
		public float X, Y, W, H;

		public float Top { get => X; set => X = value; }
		public float Left { get => Y; set => Y = value; }
		public float Right { get => W; set => W = value; }
		public float Bottom { get => H; set => H = value; }

		public float LeftRight { get => Left; set { Left = value; Right = value; } }
		public float TopBottom { get => Top; set { Top = value; Bottom = value; } }

		public float LRs => Left + Right;
		public float TBs => Top + Bottom;

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

		public Boundary( Vector2 P0, Vector2 P1 )
		{
			if ( P0.X < P1.X )
			{
				X = P0.X;
				W = P1.X - P0.X;
			}
			else
			{
				X = P1.X;
				W = P0.X - P1.X;
			}

			if ( P0.Y < P1.Y )
			{
				Y = P0.Y;
				H = P1.Y - P0.Y;
			}
			else
			{
				Y = P1.Y;
				H = P0.Y - P1.Y;
			}
		}

		public Vector2 XY { get => new Vector2( X, Y ); set { X = value.X; Y = value.Y; } }
		public Vector2 XW { get => new Vector2( X, W ); set { X = value.X; W = value.Y; } }
		public Vector2 WH { get => new Vector2( W, H ); set { W = value.X; H = value.Y; } }
		public Vector2 YH { get => new Vector2( Y, H ); set { Y = value.X; H = value.Y; } }
		public Vector4 XYWH { get => new Vector4( X, Y, W, H ); set { X = value.X; Y = value.Y; W = value.Z; H = value.W; } }

		public float XWs => X + W;
		public float YHs => Y + H;

		virtual public bool Test( Vector2 p )
		{
			return ( X <= p.X && p.X <= ( X + W ) ) && ( Y <= p.Y && p.Y <= ( Y + H ) );
		}

		virtual public bool Test( float _x, float _y )
		{
			return ( X <= _x && _x <= ( X + W ) ) && ( Y <= _y && _y <= ( Y + H ) );
		}
	}

	[DataContract, KnownType( typeof( GFProcedure ) ) ]
	abstract class GFElement
	{
		[DataMember]
		virtual public Boundary Bounds { get; set; }
		virtual public Boundary ActualBounds { get; protected set; }

		public Vector2 DrawOffset { get; set; }
		public WeakReference<Action<bool>> CCRefresh { get; private set; }

		public GFElement() => SetDefaults();

		abstract public void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev );

		protected void TriggerRedraw( bool IntegrityCheck )
		{
			if ( CCRefresh.TryGetTarget( out Action<bool> Redraw ) )
			{
				Redraw( IntegrityCheck );
			}
		}

		[OnDeserializing]
		protected void OnDeserializing( StreamingContext s ) => SetDefaults();

		virtual protected void SetDefaults()
		{
			Bounds = new Boundary();
			ActualBounds = new Boundary();
			CCRefresh = new WeakReference<Action<bool>>( null );
		}
	}

	interface IBorder
	{
		Color BDFill { get; set; }
		float BDThinkness { get; set; }
	}

	interface IForeground { Color FgFill { get; set; } }
	interface IBackground { Color BgFill { get; set; } }

	interface IGFInteractive
	{
		Action<object, GFPointerEventArgs> MouseOver { get; }
		Action<object, GFPointerEventArgs> MouseOut { get; }

		Action<object, GFPointerEventArgs> MousePress { get; }
		Action<object, GFPointerEventArgs> MouseRelease { get; }

		bool HitTest( Vector2 p );
		bool HitTest( float x, float y );
	}

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