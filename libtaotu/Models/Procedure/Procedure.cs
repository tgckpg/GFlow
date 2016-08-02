using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace libtaotu.Models.Procedure
{
    using Controls;
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
        RESULT = 256,
        CHAKRA = 512,
    }

    abstract class Procedure : ActiveData, INamable
    {
        public ProcType Type { get; protected set; }
        public string RawName { get; protected set; }

        public ProcConvoy Convoy { get; protected set; }
        public bool Faulted { get; set; }

        virtual protected Color BgColor { get { return Colors.Gray; } }
        public Brush Background { get { return new SolidColorBrush( BgColor ); } }

        virtual protected IconBase Icon { get { return new IconAtomic() { AutoScale = true }; } }
        public IconBase BlockIcon { get { return Icon; } }

        private bool _running = false;
        public bool Running
        {
            get { return _running; }
            set { _running = value; NotifyChanged( "Running" ); }
        }

        private string _TypeName;
        public string TypeName
        {
            get
            {
                if( string.IsNullOrEmpty( _TypeName ) )
                    _TypeName = ProcStrRes.Str( RawName );

                return _TypeName;
            }
            set { _TypeName = value; }
        }

        private string _Name;
        public string Name
        {
            get
            {
                if( string.IsNullOrEmpty( _Name ) )
                    _Name = ProcStrRes.Str( RawName );

                return _Name;
            }
            set
            {
                _Name = value;
                NotifyChanged( "Name" );
            }
        }

        protected StringResources ProcStrRes
        {
            get { return new StringResources( "/libtaotu/ProcItems" ); }
        }

        public Procedure( ProcType P )
        {
            Type = P;
            RawName = Enum.GetName( typeof( ProcType ), P );
        }

        protected bool TryGetConvoy( out ProcConvoy Con, Func<Procedure, ProcConvoy, bool> Tester )
        {
            // Search for usable convoy
            Con = ProcManager.TracePackage( Convoy, Tester ); 

            if( Con == null )
            {
                ProcManager.PanelMessage( this, Res.RSTR( "NoUsablePayload" ), LogType.WARNING );
                Faulted = true;
                return false;
            }

            return true;
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

