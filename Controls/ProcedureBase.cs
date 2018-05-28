using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Messaging;

namespace GFlow.Controls
{
	using BasicElements;
	using Controls.EventsArgs;
	using GraphElements;
	using Models.Procedure;

	delegate void ShowProperty( GFProcedure sender );

	class GFProcedure : GFPanel, IGFDraggable
	{
		public GFButton DragHandle { get; set; } = new GFButton();

		public void Drag( float dx, float dy, float ax, float ay )
		{
			Bounds.X += dx;
			Bounds.Y += dy;
		}

		public IGFConnector<GFProcedure> Input { get; set; }
		public IGFConnector<GFProcedure> Output { get; set; }

		public List<IGFProperty<GFProcedure>> SubProcs { get; set; } = new List<IGFProperty<GFProcedure>>();

		public Procedure Properties { get; private set; }

		public event ShowProperty OnShowProperty;

		private GFNode PropNode;
		private GFNode InputNode;
		private GFNode OutputNode;

		public GFProcedure( Procedure Proc )
		{
			Properties = Proc;
			DragHandle.Label = Proc.Name;

			InputNode = CreatePropNode( "Input" );
			InputNode.Children.Add( new GFSynapseL() );

			OutputNode = CreatePropNode( "Output" );
			OutputNode.Children.Add( new GFSynapseR() );

			PropNode = CreatePropNode( "Properties" );
			PropNode.MousePress = ( s, e ) => OnShowProperty?.Invoke( this );

			Children.Add( InputNode );
			Children.Add( OutputNode );
			Children.Add( PropNode );
		}

		private GFNode CreatePropNode( string Label )
		{
			GFNode Btn = new GFNode() { Label = Label };
			Btn.LabelFormat.FontSize = 16;
			return Btn;
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			DragHandle.Draw( ds, this, null );
			base.Draw( ds, Parent, DragHandle );
		}
	}
}