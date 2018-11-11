using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Messaging;

namespace GFlow.Controls
{
	using BasicElements;
	using GFlow.Controls.EventsArgs;
	using GraphElements;
	using Models.Interfaces;
	using Models.Procedure;

	delegate void ShowProperty( GFProcedure sender );

	class GFProcedure : GFPanel, IGFDraggable
	{
		public GFButton DragHandle => _DragHandle;

		public void Drag( float dx, float dy, float ax, float ay )
		{
			Bounds.X += dx;
			Bounds.Y += dy;
		}

		public IGFConnector<GFProcedure> Input { get; set; }
		public IGFConnector<GFProcedure> Output { get; set; }

		public Procedure Properties { get; private set; }

		public event ShowProperty OnShowProperty;

		private Dictionary<IProcessNode, GFNode> ProcessNodes;

		private GFTextButton _DragHandle = new GFTextButton();
		private GFPanel PNPanel;
		private GFNode PropNode;
		private GFNode InputNode;
		private GFNode OutputNode;

		public GFProcedure( Procedure Proc )
		{
			Properties = Proc;

			// Drag Title
			_DragHandle.Label = Proc.Name;
			GFPanel DTPanel = new GFPanel();
			DTPanel.Orientation = Orientation.Horizontal;

			GFTextButton DeleteBtn = new GFTextButton();
			DeleteBtn.LabelFormat.FontFamily = "Segoe MDL2 Assets";
			DeleteBtn.Label = "\uE74D";
			DeleteBtn.Bounds.W = 18;
			DeleteBtn.Padding.Top = 8;
			DeleteBtn.Padding.Bottom = 2;
			DeleteBtn.SetRed();
			DeleteBtn.MousePress = SelfDestruct;

			// IO Nodes
			InputNode = CreatePropNode( "Input" );
			InputNode.Children.Add( new GFSynapseL() );
			OutputNode = CreatePropNode( "Output" );
			OutputNode.Children.Add( new GFSynapseR() );

			// SubProc Nodes
			PNPanel = new GFPanel();

			// Prop Node
			PropNode = CreatePropNode( "Properties" );
			PropNode.MousePress = ( s, e ) => OnShowProperty?.Invoke( this );

			DTPanel.Children.Add( DragHandle );
			DTPanel.Children.Add( DeleteBtn );
			Children.Add( DTPanel );
			Children.Add( InputNode );
			Children.Add( OutputNode );
			Children.Add( PNPanel );
			Children.Add( PropNode );

			if( Proc is IProcessList ProcList )
			{
				BindProcessNodes( ProcList );
			}
		}

		private void SelfDestruct( object sender, GFPointerEventArgs e )
		{
			( ( GFDrawBoard ) sender ).Remove( this );
			TriggerRedraw( true );
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
						PNPanel.Children.Remove( RmNode );
						ProcessNodes.Remove( PN );
					}
				}

				foreach ( IProcessNode PN in PNodes )
				{
					if ( !ProcessNodes.ContainsKey( PN ) )
					{
						GFNode GNode = CreatePropNode( PN.Key );
						GNode.SetDarkTheme( 0xFF101020 );

						if ( PN is IGFLabelOwner )
							GNode.SetLabelOwner( ( IGFLabelOwner ) PN );

						GNode.Children.Add( new GFSynapseR() );
						ProcessNodes.Add( PN, GNode );
						PNPanel.Children.Add( GNode );
					}
				}
			}
		}

		private GFNode CreatePropNode( string Label )
		{
			GFNode Btn = new GFNode() { Label = Label };
			Btn.HAlign = HorizontalAlignment.Stretch;
			Btn.LabelFormat.FontSize = 16;
			return Btn;
		}

	}
}