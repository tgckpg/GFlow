using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;

namespace libtaotu.Models.Procedure
{
    internal enum ProcType
    {
        URLLIST = 1,
        GENERATOR = 2,
        FIND = 4,
        MARK = 8,
        EXTRACT = 16,
        DUMMY = 32,
        INSTRUCTION = 64,
        PASSTHRU = 128,
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

        virtual public async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            return await Task.Run( () => this.Convoy = Convoy );
        }

        virtual public void ReadParam( XParameter Param )
        {
            string PName = Param.GetValue( "Name" );
            if ( PName != null ) Name = PName;
        }

        virtual public XParameter ToXParam()
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

