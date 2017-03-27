using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace libtaotu.Controls
{
	using Crawler;
	using Models.Procedure;
	using Pages;

	class ProcManager : ActiveData
	{
		public static readonly string ID = typeof( ProcManager ).Name;

		public ObservableCollection<Procedure> ProcList { get; private set; }
		public bool HasProcedures { get { return 0 < ProcList.Count; } }
		public bool Async { get; set; }

		private string _Id;
		public string GUID
		{
			get { return _Id; }
			set { _Id = value; }
		}

		// Used to locate specific procedure in chain
		private int From = 0;
		private int To = 0;

		public static ProcConvoy TracePackage( ProcConvoy Start, Func<Procedure, ProcConvoy, bool> Verifier )
		{
			if ( Start == null ) return null;

			Procedure Disp = Start.Dispatcher;
			ProcConvoy Convoy = Start;
			while ( Convoy != null )
			{
				if( Verifier( Disp, Convoy ) )
				{
					return Convoy;
				}

				// Dispatcher's Convoy
				Convoy = Disp.Convoy;
				if ( Disp.Convoy == null ) break;

				// Dispatcher's Convoy's Dispatcher...
				Disp = Disp.Convoy.Dispatcher;
			}

			return null;
		}

		public static void PanelMessage( string ID, string Mesg, LogType LogLevel )
		{
			Logger.Log( ID, Mesg, LogLevel );
			MessageBus.Send(
				typeof( ProceduresPanel )
				, Mesg
				, new ProceduresPanel.PanelLog() { LogType = LogLevel, ID = ID }
			);
		}

		public static void PanelMessage( Procedure P, string Mesg, LogType LogLevel )
		{
			string Tag = P.Name == P.TypeName
				? P.Name
				: string.Format( "[{0}({1})]", P.Name, P.RawName )
				;

			PanelMessage( Tag, Mesg, LogLevel );
		}

		public ProcManager()
		{
			ProcList = new ObservableCollection<Procedure>();
			_Id = Guid.NewGuid().ToString();
			Async = false;
		}

		public ProcManager( XParameter Param )
			:this()
		{
			if ( Param == null ) return;
			ReadParam( Param );
		}

		public Procedure NewProcedure( ProcType P )
		{
			Procedure Proc = null;
			switch ( P )
			{
				case ProcType.URLLIST:
					Proc = new ProcUrlList();
					break;
				case ProcType.FIND:
					Proc = new ProcFind();
					break;
				case ProcType.GENERATOR:
					Proc = new ProcGenerator();
					break;
				case ProcType.RESULT:
					Proc = new ProcResult();
					break;
				case ProcType.CHAKRA:
					Proc = new ProcChakra();
					break;
				case ProcType.ENCODING:
					Proc = new ProcEncoding();
					break;
				case ProcType.PARAMETER:
					Proc = new ProcParameter();
					break;
				case ProcType.EXTRACT:
					Proc = ProcExtract.Create();
					break;
				case ProcType.MARK:
					Proc = ProcMark.Create();
					break;
				case ProcType.LIST:
					Proc = ProcListLoader.Create();
					break;
			}

			ProcList.Add( Proc );
			return Proc;
		}

		public void ActiveRange( int From = 0, int To = 0 )
		{
			this.From = From;
			this.To = To;
		}

		public ProceduralSpider CreateSpider()
		{
			IEnumerable<Procedure> SelectedProcs = ProcList;

			if( 0 < From )
			{
				SelectedProcs = SelectedProcs.Skip( From );
			}

			if( 0 < To )
			{
				SelectedProcs = SelectedProcs.Take( To - From );
			}

			return new ProceduralSpider( SelectedProcs );
		}

		public void RemoveProcedure( Procedure P )
		{
			ProcList.Remove( P );
		}

		public void Move( Procedure P, int dir )
		{
			int i = ProcList.IndexOf( P );
			if ( ( 0 < i && dir < 0 ) || ( -1 < i && 0 < dir ) )
			{
				i += dir;
				if ( -1 < i && i < ProcList.Count )
				{
					ProcList.Remove( P );
					ProcList.Insert( i, P );
				}
			}
		}

		public void ReadParam( XParameter List )
		{
			XParameter[] ProcParams = List.Parameters( "ProcType" );
			Async = List.GetBool( "Async", false );
			GUID = List.GetValue( "Guid" );

			Type PType = typeof( ProcType );
			IEnumerable<ProcType> P = Enum.GetValues( PType ).Cast<ProcType>();
			foreach( XParameter Param in ProcParams )
			{
				string ProcName = Param.GetValue( "ProcType" );
				ProcType Proc = P.First( x => Enum.GetName( PType, x ) == ProcName );

				Procedure NProc = NewProcedure( Proc );
				NProc.ReadParam( Param );
			}
		}

		public XParameter ToXParam( string ProcId = null )
		{
			XParameter Param = new XParameter( "Procedures" );
			Param.SetValue( new XKey[] {
				new XKey( "Async", Async )
				, new XKey( "Guid", ProcId == null ? GUID : ProcId )
			} );

			int i = 0;
			foreach ( Procedure P in ProcList )
			{
				XParameter ProcParam = P.ToXParam();
				ProcParam.Id = "Proc" + ( i++ );
				ProcParam.SetValue( new XKey( "ProcType", P.RawName ) );
				Param.SetParameter( ProcParam );
			}

			return Param;
		}
	}
}