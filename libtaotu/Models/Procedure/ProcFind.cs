using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace libtaotu.Models.Procedure
{
    using Controls;

    enum FindMode
    {
        MATCH = 1, REPLACE = 2
    }

    class ProcFind : Procedure
    {
        public static readonly string ID = typeof( ProcFind ).Name;

        public FindMode Mode { get; set; }

        public string TestLink { get; set; }

        public HashSet<string> FilteredContent { get; private set; }
        public ObservableCollection<RegItem> RegexPairs { get; private set; }

        public string RawModeName { get; private set; }
        public string ModeName { get; private set; }

        protected override IconBase Icon { get { return new IconSearch() { AutoScale = true }; } }
        protected override Color BgColor { get { return Colors.Purple; } }

        public ProcFind()
            : base( ProcType.FIND )
        {
            FilteredContent = new HashSet<string>();
            RegexPairs = new ObservableCollection<RegItem>();

            SetMode( FindMode.MATCH );
        }

        public void RemoveRegex( RegItem Item )
        {
            RegexPairs.Remove( Item );
            NotifyChanged( "RegexPairs" );
        }

        public void ToggleMode()
        {
            SetMode( Mode == FindMode.REPLACE ? FindMode.MATCH : FindMode.REPLACE );
        }

        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );

            ProcConvoy UsableConvoy;
            if ( !TryGetConvoy( out UsableConvoy, ( P, C ) =>
            {
                return C.Payload is IEnumerable<IStorageFile>
                || C.Payload is string;
            }
            ) ) return Convoy;

            List<IStorageFile> TargetFiles = new List<IStorageFile>();
            if ( UsableConvoy.Payload is string )
            {
                IStorageFile ISF = await AppStorage.MkTemp();
                TargetFiles.Add( await FilterContent( ISF, UsableConvoy.Payload as string ) );
            }
            else
            {
                IEnumerable<IStorageFile> SrcFiles = UsableConvoy.Payload as IEnumerable<IStorageFile>;

                foreach ( IStorageFile ISF in SrcFiles )
                {
                    TargetFiles.Add( await FilterContent( ISF ) );
                }
            }

            return new ProcConvoy( this, TargetFiles );
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcFind( this ) );
        }

        public async Task<IStorageFile> FilterContent( IStorageFile Src )
        {
            return await FilterContent( await AppStorage.MkTemp(), await Src.ReadString() );
        }

        public async Task<IStorageFile> FilterContent( IStorageFile SF, string Content )
        {
            IEnumerable<string> str = Parse( Content );

            if ( await SF.WriteString( string.Join( "\n", str ) ) )
            {
                return SF;
            }

            return null;
        }

        private IEnumerable<string> Parse( string v )
        {
            try
            {
                bool RegExed = false;

                SortedDictionary<int, string> OrderedMatchings = new SortedDictionary<int, string>();

                foreach ( RegItem R in RegexPairs )
                {
                    if ( !R.Enabled ) continue;
                    RegExed = true;

                    if( Mode == FindMode.REPLACE )
                    {
                        v = R.RegExObj.Replace(
                            v, x => string.Format(
                                R.Format.Unescape()
                                , x.Groups
                                .Cast<Group>()
                                .Select( g => g.Value )
                                .ToArray()
                            )
                        );
                        continue;
                    }

                    MatchCollection matches = R.RegExObj.Matches( v );

                    foreach ( Match match in matches )
                    {
                        string formatted = string.Format(
                            R.Format.Unescape()
                            , match.Groups
                                .Cast<Group>()
                                .Select( g => g.Value )
                                .ToArray()
                        );

                        OrderedMatchings.Add( match.Index, formatted );
                    }
                }

                if ( RegExed )
                {
                    if( Mode == FindMode.MATCH )
                    {
                        return OrderedMatchings.Values;
                    }
                    else
                    {
                        return new string[] { v };
                    }
                }
            }
            catch ( Exception ex )
            {
                ProcManager.PanelMessage( this, ex.Message, LogType.INFO );
            }

            return new string[] { v };
        }

        private void SetMode( string name )
        {
            switch( name )
            {
                case "REPLACE":
                    SetMode( FindMode.REPLACE );
                    break;
                case "MATCH":
                default:
                    SetMode( FindMode.MATCH );
                    break;
            }
        }

        private void SetMode( FindMode Mode )
        {
            this.Mode = Mode;
            RawModeName = Enum.GetName( typeof( FindMode ), Mode );
            ModeName = ProcStrRes.Str( RawModeName );

            foreach ( RegItem R in RegexPairs ) R.Validate( Mode );

            NotifyChanged( "RawModeName", "ModeName" );
        }

        public override void ReadParam( XParameter Param )
        {
            base.ReadParam( Param );

            XParameter[] RegParams = Param.GetParametersWithKey( "i" );
            TestLink = Param.GetValue( "TestLink" );
            SetMode( Param.GetValue( "Mode" ) );

            foreach ( XParameter RegParam in RegParams )
            {
                RegexPairs.Add( new RegItem( RegParam ) );
            }
        }

        public override XParameter ToXParam()
        {
            XParameter Param = base.ToXParam();

            Param.SetValue( new XKey[] {
                new XKey( "TestLink", TestLink )
                , new XKey( "Mode", RawModeName )
            } );

            int i = 0;
            foreach( RegItem R in RegexPairs )
            {
                XParameter RegParam = R.ToXParam();
                RegParam.ID += i;
                RegParam.SetValue( new XKey( "i", i++ ) );

                Param.SetParameter( RegParam );
            }

            return Param;
        }

        public class RegItem : ActiveData
        {
            public string Pattern { get; set; }
            public string Format { get; set; }

            public Regex RegExObj
            {
                get
                {
                    if( string.IsNullOrEmpty( Pattern ) )
                    {
                        return new Regex( @"[^\s\S]", RegexOptions.Multiline );
                    }
                    return new Regex( Pattern, RegexOptions.Multiline );
                }
            }

            private bool _valid = true;
            public bool Valid
            {
                get { return _valid; }
                set
                {
                    _valid = value;
                    NotifyChanged( "Valid" );
                }
            }

            private bool _enabled = false;

            virtual public bool Enabled
            {
                get
                {
                    if ( string.IsNullOrEmpty( Pattern ) || !Valid ) return false;
                    return _enabled;
                }
                set
                {
                    _enabled = value;
                    NotifyChanged( "Enabled" );
                }
            }

            public RegItem()
            {
                this.Pattern = "";
                this.Format = "";
            }

            public RegItem( string pattern, string format, bool enable )
            {
                this.Pattern = pattern;
                this.Format = format;
                this.Enabled = enable;
            }

            public RegItem( XParameter Param )
            {
                Pattern = Param.GetValue( "Pattern" );
                Format = Param.GetValue( "Format" );
                Enabled = Param.GetBool( "Enabled" );
            }

            public bool Validate( FindMode Mode = FindMode.MATCH )
            {
                try
                {
                    Regex RegEx = RegExObj;
                    if( Mode == FindMode.MATCH )
                    {
                        string.Format( Format.Trim(), RegEx.GetGroupNames() );
                    }
                    Valid = true;
                }
                catch( Exception ex )
                {
                    ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                    Valid = false;
                }

                return Valid;
            }

            virtual public XParameter ToXParam()
            {
                XParameter Param = new XParameter( "RegItem" );
                Param.SetValue( new XKey[] {
                    // Pattern will be the key identifier
                    new XKey( "Pattern", Pattern )
                    , new XKey( "Format", Format )
                    , new XKey( "Enabled", Enabled )
                } );

                return Param;
            }
        }
    }
}
