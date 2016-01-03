using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using System.Reflection;

namespace libtaotu.Models.Procedure
{
    class ProcFind : Procedure
    {
        public static readonly string ID = typeof( ProcFind ).Name;

        public HashSet<string> FilteredContent { get; private set; }

        public ObservableCollection<RegItem> RegexPairs { get; private set; }

        public ProcFind()
        {
            Type = ProcType.FIND;
            FilteredContent = new HashSet<string>();
            RegexPairs = new ObservableCollection<RegItem>();
        }

        public void ValidateRegex( RegItem R )
        {
            try
            {
                Regex RegEx = R.RegExObj;
                string.Format( R.Format.Trim(), RegEx.GetGroupNames() );
                R.Valid = true;
            }
            catch( Exception ex )
            {
                Logger.Log( ID, ex.Message, LogType.INFO );
                R.Valid = false;
            }
        }

        public void RemoveRegex( RegItem Item )
        {
            RegexPairs.Remove( Item );
            NotifyChanged( "RegexPairs" );
        }

        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );
            IEnumerable<string> ThingsToMatch = TryFindPayload( Convoy );

            if( ThingsToMatch == null )
            {
                Logger.Log( ID, "Unable to find a usable payload, skipping this step", LogType.WARNING );
                return null;
            }

            throw new NotImplementedException();
        }

        private IEnumerable<string> TryFindPayload( ProcConvoy convoy )
        {
            if ( convoy == null ) return null;

            while( convoy.Payload as IEnumerable<string> == null && convoy.Dispatcher != null )
            {
                convoy = convoy.Dispatcher.Convoy;
            }

            return convoy.Payload as IEnumerable<string>;
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcFind( this ) );
        }

        public async Task<IStorageFile> FilterContent( IStorageFile Src )
        {
            StorageFile SF = await AppStorage.MkTemp();

            IEnumerable<string> str = Parse( await Src.ReadString() );

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
                List<string> MatchingResult = new List<string>();
                bool RegExed = false;
                foreach ( RegItem R in RegexPairs )
                {
                    if ( !R.Enabled ) continue;
                    RegExed = true;

                    MatchCollection matches = R.RegExObj.Matches( v );

                    foreach ( Match match in matches )
                    {
                        string[] s = new string[ match.Groups.Count ];

                        int i = 0;
                        foreach ( Group m in match.Groups ) s[ i++ ] = m.Value;

                        string formatted = string.Format( R.Format, s );

                        MatchingResult.Add( formatted );
                    }
                }

                if( RegExed ) return MatchingResult;
            }
            catch ( Exception ex )
            {
            }

            return new string[] { v };
        }

        public class RegItem : ActiveData
        {
            public string Pattern;
            public string Format;

            public Regex RegExObj
            {
                get { return new Regex( Pattern, RegexOptions.Multiline ); }
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
            public bool Enabled
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
        }
    }
}
