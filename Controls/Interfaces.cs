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
	struct Boundary { public float X, Y, W, H; }

	interface IForeground { Color FGFill { get; set; } }
	interface IBackground { Color BGFill { get; set; } }

	interface IGFElement
	{
		Vector2 P { get; set; }
		void Draw( CanvasDrawingSession ds );
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

	interface IGFDraggable : IGFElement
	{
		Boundary DragHandle { get; set; }
		void Drag( float x, float y );
	}

}