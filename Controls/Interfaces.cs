using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls
{
	interface IGFBoundary
	{
		Vector4 Bounds { get; set; }
	}

	interface IGFButton : IGFBoundary
	{

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

	interface IGFDraggable
	{
		float X { get; set; }
		float Y { get; set; }

		IGFBoundary DragHandle { get; set; }

		void Drag( float x, float y );
	}

}