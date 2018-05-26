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

	class GFSynapse : GFButton
	{
		public Color SNFill { get; set; } = Colors.Black;

		public GFSynapse()
		{
			ActualBounds.W = 20;
			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
		}

		private static void _MouseOver( GFButton Target )
		{
			( ( GFSynapse ) Target ).SNFill = Colors.OrangeRed;
		}

		private static void _MouseOut( GFButton Target )
		{
			( ( GFSynapse ) Target ).SNFill = Colors.Black;
		}
	}

	class GFSynapseL : GFSynapse
	{
		// This assumes Parent is always present
		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			ActualBounds.YH = Parent.ActualBounds.YH;
			ActualBounds.X = Parent.ActualBounds.X - 20;

			float HH = Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H;
			float MX = ActualBounds.X + 15;

			ds.DrawLine( MX, HH, MX + 5, HH, SNFill );
			ds.DrawLine( MX, HH, MX - 5, HH - 5, SNFill );
			ds.DrawLine( MX, HH, MX - 5, HH + 5, SNFill );
		}
	}

	class GFSynapseR : GFSynapse
	{
		// This assumes Parent is always present
		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			ActualBounds.YH = Parent.ActualBounds.YH;
			ActualBounds.X = Parent.ActualBounds.XWs;

			float HH = Parent.ActualBounds.Y + 0.5f * Parent.ActualBounds.H;
			float MX = Parent.ActualBounds.XWs + 5;

			ds.DrawLine( MX, HH, MX - 5, HH, SNFill );
			ds.DrawLine( MX, HH, MX + 5, HH - 5, SNFill );
			ds.DrawLine( MX, HH, MX + 5, HH + 5, SNFill );
		}
	}

	class GFNode : GFButton, IGFContainer
	{
		// For hit tests
		public IList<GFElement> Children { get; set; }

		public GFNode()
			: base()
		{
			BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
			Children = new List<GFElement>();

			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
		}

		private static void _MouseOver( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xFF, 0xD0, 0xD0, 0xD0 );
		}

		private static void _MouseOut( GFButton Target )
		{
			Target.BGFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
		}

		public void Add( GFElement e ) => throw new NotSupportedException();
		public void Remove( GFElement e ) => throw new NotSupportedException();

	}
}