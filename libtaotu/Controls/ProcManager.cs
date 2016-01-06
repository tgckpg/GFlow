﻿using System;
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
    using Models.Procedure;
    using Pages;

    class ProcManager : ActiveData
    {
        public static readonly string ID = typeof( ProcManager ).Name;

        public ObservableCollection<Procedure> ProcList { get; private set; }
        public bool Async { get; set; }

        private Guid _Guid;
        public Guid GUID { get { return _Guid; } }

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
            _Guid = Guid.NewGuid();
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
                if ( 0 < To && To < i ) continue;
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
            XParameter[] ProcParams = List.GetParametersWithKey( "ProcType" );
            Async = List.GetBool( "Async", false );
            if( !Guid.TryParse( "NAN", out _Guid ) )
            {
                _Guid = Guid.NewGuid();
            }

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
            Param.SetValue( new XKey[] {
                new XKey( "Async", Async )
                , new XKey( "Guid", GUID )
            } );

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
