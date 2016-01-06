using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace libtaotu.Controls
{
    using Models.Procedure;
    using Pages;

    class ProcManager
    {
        public static readonly string ID = typeof( ProcManager ).Name;

        public ObservableCollection<Procedure> ProcList { get; private set; }

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
            MessageBus.SendUI(
                new Message(
                    typeof( ProceduresPanel )
                    , Mesg
                    , new ProceduresPanel.PanelLog() { LogType = LogLevel, ID = ID }
                )
            );
        }

        public ProcManager()
        {
            ProcList = new ObservableCollection<Procedure>();
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
                case ProcType.EXTRACT:
                    Proc = ProcExtract.Create();
                    break;
                case ProcType.MARK:
                    Proc = ProcMark.Create();
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

        public async Task<ProcConvoy> Run( ProcConvoy Convoy = null )
        {
            ProcConvoy Conveying = Convoy;
            if( ProcList.Count == 0 )
            {
                PanelMessage( ID, "Proc list is empty, nothing to do", LogType.INFO );
                return Convoy;
            }

            int i = 0;
            foreach ( Procedure Proc in ProcList )
            {
                if ( 0 < To && i < To ) continue;
                if ( ++i < From ) continue;

                PanelMessage( ID, "Running " + Proc.TypeName, LogType.INFO );

                try
                {
                    Proc.Running = true;
                    ProcConvoy Received = await Proc.Run( Conveying );
                    Conveying = Received;
                    Proc.Running = false;
                }
                catch ( Exception ex )
                {
                    PanelMessage( ID, ex.Message, LogType.ERROR );
                    Proc.Running = false;
                    Proc.Faulted = true;
                    return null;
                }
            }

            PanelMessage( ID, "Cycle Completed", LogType.INFO );
            return Conveying;
        }

        public void RemoveProcedure( Procedure P )
        {
            ProcList.Remove( P );
        }

        public void ReadParam( XParameter List )
        {
            XParameter[] ProcParams = List.GetParametersWithKey( "ProcType" );

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

        public XParameter ToXParam()
        {
            XParameter Param = new XParameter( "Procedures" );
            int i = 0;
            foreach ( Procedure P in ProcList )
            {
                XParameter ProcParam = P.ToXParem();
                ProcParam.ID = "Proc" + ( i++ );
                ProcParam.SetValue( new XKey( "ProcType", P.RawName ) );
                Param.SetParameter( ProcParam );
            }

            return Param;
        }
    }
}
