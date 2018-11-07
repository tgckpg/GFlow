using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Messaging;

namespace GFlow.Controls
{
	using BasicElements;
	using GraphElements;
	using Models.Interfaces;
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

		private Dictionary<IProcessNode, GFNode> ProcessNodes;

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

			if( Proc is IProcessList ProcList )
			{
				BindProcessNodes( ProcList );
			}
		}

		private void BindProcessNodes( IProcessList ProcList )
		{
			ProcessNodes = new Dictionary<IProcessNode, GFNode>();

			if( ProcList.ProcessNodes is INotifyCollectionChanged ObsProcList )
			{
				ObsProcList.CollectionChanged += ObsProcList_CollectionChanged;
			}

			CreateProcessNodes( ProcList.ProcessNodes );
		}

		private void ObsProcList_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			CreateProcessNodes( ( IList<IProcessNode> ) sender );
			TriggerRedraw( true );
		}

		private void CreateProcessNodes( IList<IProcessNode> PNodes )
		{
			lock ( ProcessNodes )
			{
				foreach ( IProcessNode PN in ProcessNodes.Keys.ToArray() )
				{
					if ( !PNodes.Contains( PN ) )
					{
						GFNode RmNode = ProcessNodes[ PN ];
						Children.Remove( RmNode );
						ProcessNodes.Remove( PN );
					}
				}

				foreach ( IProcessNode PN in PNodes )
				{
					if ( !ProcessNodes.ContainsKey( PN ) )
					{
						GFNode GNode = CreatePropNode( PN.Key );

						if ( PN is IGFLabelOwner )
							GNode.SetLabelOwner( ( IGFLabelOwner ) PN );

						GNode.Children.Add( new GFSynapseR() );
						ProcessNodes.Add( PN, GNode );
						Children.Add( GNode );
					}
				}
			}
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