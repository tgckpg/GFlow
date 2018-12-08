using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;

namespace GFlow.Controls
{
	using BasicElements;
	using EventsArgs;
	using GraphElements;
	using Models.Interfaces;
	using Models.Procedure;

	delegate void ShowProperty( GFProcedure Target );
	delegate void TestRun( GFProcedure Target );
	delegate void ProcRemove( GFProcedure Target );

	[DataContract]
	class GFProcedure : GFPanel, IGFDraggable
	{
		public GFButton DragHandle => _DragHandle;

		public void Drag( float dx, float dy, float ax, float ay )
		{
			Bounds.X += dx;
			Bounds.Y += dy;
		}

		private bool _IsStart = false;

		[DataMember]
		public bool IsStart
		{
			get => _IsStart;
			set
			{
				_IsStart = value;
				if ( value )
				{
					SPButton.SetDarkTheme( 0xFF111144 );
				}
				else
				{
					SPButton.SetLightThemeAlt( 0xFF111144 );
				}
			}
		}

		public Procedure Properties { get; private set; }

		public event ShowProperty OnShowProperty;
		public event TestRun OnTestRun;
		public event ProcRemove OnRemove;

		public GFSynapse Transmitter => ( GFTransmitter ) OutputNode.Children.First( x => x is GFTransmitter );
		public GFSynapse Receptor => ( GFReceptor ) InputNode.Children.First( x => x is GFReceptor );

		private Dictionary<IProcessNode, GFNode> ProcessNodes;

		private GFTextButton _DragHandle;
		private GFTextButton SPButton;
		private GFTextButton TestRunBtn;
		private GFPanel PNPanel;
		private GFNode PropNode;
		private GFNode InputNode;
		private GFNode OutputNode;

		public GFProcedure( Procedure Proc )
		{
			Properties = Proc;
			InitProc();
		}

		public Procedure GetProcedure()
		{
			Procedure Proc = ( Procedure ) Activator.CreateInstance( Properties.GetType() );
			Proc.ReadParam( Properties.ToXParam() );
			return Proc;
		}

		public GFTransmitter GetTransmitter( IProcessNode PN )
		{
			lock ( ProcessNodes )
			{
				if ( ProcessNodes.ContainsKey( PN ) )
				{
					return ( GFTransmitter ) ProcessNodes[ PN ].Children.First( x => x is GFTransmitter );
				}
			}
			return null;
		}

		protected override void SetDefaults()
		{
			base.SetDefaults();
			// Drag Title
			GFPanel TitlePanel = new GFPanel();
			TitlePanel.Orientation = Orientation.Horizontal;
			_DragHandle = new GFTextButton();
			_DragHandle.Bounds.W -= 32;

			GFTextButton DeleteBtn = CreateIconButton( "\uE74D", 0xFFAA0000 );
			DeleteBtn.MousePress = SelfDestruct;

			TestRunBtn = CreateIconButton( "\uE768", 0xFF008800 );
			TestRunBtn.MousePress = ( s, e ) => OnTestRun?.Invoke( this );

			// Starting Point Button
			SPButton = CreateIconButton( "\uEC43", 0xFF000044 );
			SPButton.MousePress = SetStart;

			// IO Nodes
			InputNode = CreatePropNode( "Input" );
			InputNode.Children.Add( new GFReceptor( this ) );
			OutputNode = CreatePropNode( "Output" );
			OutputNode.Children.Add( new GFTransmitter( this ) );

			// SubProc Nodes
			PNPanel = new GFPanel();

			// Prop Node
			PropNode = CreatePropNode( "Properties" );
			PropNode.MousePress = ( s, e ) => OnShowProperty?.Invoke( this );

			TitlePanel.Children.Add( DragHandle );
			TitlePanel.Children.Add( SPButton );
			TitlePanel.Children.Add( TestRunBtn );
			TitlePanel.Children.Add( DeleteBtn );
			Children.Add( TitlePanel );
			Children.Add( InputNode );
			Children.Add( OutputNode );
			Children.Add( PNPanel );
			Children.Add( PropNode );
		}

		private void InitProc()
		{
			_DragHandle.SetLabelOwner( Properties );

			if ( Properties.PropertyPage == null )
				Children.Remove( PropNode );

			if ( Properties is IProcessList ProcList )
			{
				BindProcessNodes( ProcList );
			}
		}

		private void SelfDestruct( object sender, GFPointerEventArgs e )
		{
			( ( GFDrawBoard ) sender ).Remove( this );
			OnRemove?.Invoke( this );
			TriggerRedraw( true );
		}

		private void SetStart( object sender, GFPointerEventArgs e )
		{
			GFDrawBoard DrawBoard = ( GFDrawBoard ) sender;
			bool _OStart = IsStart;

			DrawBoard.Find<GFProcedure>( 1 ).ExecEach( x => x.IsStart = false );

			IsStart = !_OStart;
			TriggerRedraw( false );
		}

		private void BindProcessNodes( IProcessList ProcList )
		{
			ProcessNodes = new Dictionary<IProcessNode, GFNode>();

			if ( ProcList.ProcessNodes is INotifyCollectionChanged ObsProcList )
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

						if ( PN is INamable )
							GNode.SetLabelOwner( ( INamable ) PN );

						GNode.Children.Add( new GFTransmitter( this )
						{
							SynapseType = SynapseType.BRANCH
							, Dendrite00 = PN
						} );

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

		private GFTextButton CreateIconButton( string Glyph, uint Color )
		{
			GFTextButton Btn = new GFTextButton();
			Btn.LabelFormat.FontFamily = "Segoe MDL2 Assets";
			Btn.Label = Glyph;
			Btn.Bounds.W = 16;
			Btn.Padding.Top = 8;
			Btn.Padding.Bottom = 2;
			Btn.SetLightThemeAlt( Color );
			return Btn;
		}

		private string _SDataProcName;

		[DataMember]
		private string SDataProcName
		{
			get => Properties?.RawName ?? _SDataProcName;
			set => _SDataProcName = value;
		}

		[DataMember]
		private string SDataProcVal
		{
			get
			{
				XRegistry XReg = new XRegistry( "<SeData />", null, false );
				XReg.SetParameter( Properties.ToXParam() );
				return XReg.ToString();
			}

			set
			{
				Properties = GFProcedureList.Create( SDataProcName );
				Properties.ReadParam( new XRegistry( value, null, false ).Parameter( SDataProcName ) );
				InitProc();
			}
		}

	}
}