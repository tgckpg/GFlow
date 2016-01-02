using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Net.Astropenguin.Logging;

namespace libtaotu.Models.Procedure
{
    class ProcUrlList : Procedure
    {
        public static readonly string ID = typeof( ProcUrlList ).Name;

        public HashSet<string> Urls { get; private set; }

        public ProcUrlList()
        {
            Type = ProcType.URLLIST;
            Urls = new HashSet<string>();
            Convoy = new ProcConvoy( this, Urls );
        }

        public override Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            if ( Convoy != null )
            {
                Logger.Log( ID, "A Convoy? Nothing to do here", LogType.INFO );
            }

            return Task.Run( () => this.Convoy );
        }

        public override async Task Edit()
        {
            await Net.Astropenguin.Helpers.Popups.ShowDialog( new Dialogs.EditProcUrlList( this ) );
        }
    }
}
