using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Net.Astropenguin.Logging;

namespace libtaotu.Models.Procedure
{
    class ProcFind : Procedure
    {
        public static readonly string ID = typeof( ProcFind ).Name;

        public HashSet<string> FilteredContent { get; private set; }

        public Regex RegEx { get; private set; }

        public ProcFind()
        {
            Type = ProcType.FIND;
            FilteredContent = new HashSet<string>();
        }

        public bool SetRegex( string pattern )
        {
            try
            {
                RegEx = new Regex( pattern );
                return true;
            }
            catch( Exception ex )
            {
                Logger.Log( ID, ex.Message, LogType.INFO );
            }

            return false;
        }

        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );
            IEnumerable<string> ThingsToMatch = TryFindPayload( Convoy );

            if( ThingsToMatch == null )
            {
                Logger.Log( ID, "Cannot find a usable payload, skipping this step", LogType.WARNING );
                return null;
            }

            List<string> Findings = new List<string>();
            foreach( string Matchee in ThingsToMatch )
            {
                Match matches = RegEx.Match( Matchee );
                throw new NotImplementedException();
            }

            return new ProcConvoy( this, Findings );
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

        public override Task Edit()
        {
            throw new NotImplementedException();
        }
    }
}
