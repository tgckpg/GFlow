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

    abstract class Procedure : ActiveData, INamable
    {
        public ProcType Type { get; protected set; }
        public string RawName { get; protected set; }
        public string TypeName { get; protected set; }

        public ProcConvoy Convoy { get; protected set; }
        public bool Faulted { get; set; }

        private bool _running = false;
        public bool Running
        {
            get { return _running; }
            set { _running = value; NotifyChanged( "Running" ); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyChanged( "Name" );
            }
        }

        protected static StringResources ProcStrRes;

        public Procedure( ProcType P )
        {
            Type = P;

            if ( ProcStrRes == null ) ProcStrRes = new StringResources( "/libtaotu/ProcItems" );
            RawName = Enum.GetName( typeof( ProcType ), P );
            TypeName = ProcStrRes.Str( RawName );
            Name = TypeName;
        }

        virtual public Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            return Task.Run( () => this.Convoy = Convoy );
        }

        virtual public void ReadParam( XParameter Param )
        {
            string PName = Param.GetValue( "Name" );
            if ( PName != null ) Name = PName;
        }

        virtual public XParameter ToXParem()
        {
            XParameter Param = new XParameter( RawName );
            if( Name != TypeName )
            {
                Param.SetValue( new XKey( "Name", Name ) );
            }

            return Param;
        }

        abstract public Task Edit();
    }
}

