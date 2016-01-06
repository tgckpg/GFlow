using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace libtaotu.Models.Procedure
{
    using Pages;

    internal enum ProcType
    {
        URLLIST = 1,
        FIND = 2,
        MARK = 4,
        EXTRACT = 8,
        PAUSE = 16,
    }

    abstract class Procedure : ActiveData
    {
        public ProcType Type { get; protected set; }
        public string RawName { get; protected set; }
        public string TypeName { get; protected set; }

        public bool Faulted { get; set; }

        private bool _running = false;
        public bool Running
        {
            get { return _running; }
            set { _running = value; NotifyChanged( "Running" ); }
        }

        public ProcConvoy Convoy { get; protected set; }

        private static StringResources ProcStrRes;

        public Procedure( ProcType P )
        {
            Type = P;

            if ( ProcStrRes == null ) ProcStrRes = new StringResources( "/libtaotu/ProcItems" );
            RawName = Enum.GetName( typeof( ProcType ), P );
            TypeName = ProcStrRes.Str( RawName );
        }

        virtual public Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            return Task.Run( () => this.Convoy = Convoy );
        }

        abstract public Task Edit();
        abstract public void ReadParam( XParameter Param );
        abstract public XParameter ToXParem();
    }
}

