using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace libtaotu.Models.Procedure
{
    using Controls;

    class ProcResult : Procedure
    {
        public static readonly string ID = typeof( ProcResult ).Name;

        protected override Color BgColor { get { return Colors.Black; } }
        protected override IconBase Icon { get { return new IconEEye() { AutoScale = true }; } }

        public ProcResult() : base( ProcType.RESULT ) { }
        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );

            ProcConvoy UsableConvoy = ProcManager.TracePackage(
                Convoy, ( P, C ) =>
                {
                    return C.Payload is IEnumerable<IStorageFile>
                    || C.Payload is string;
                }
            );

            if( UsableConvoy == null )
            {
                ProcManager.PanelMessage( this, Res.RSTR( "NoUsablePayload" ), LogType.WARNING );
                Faulted = true;
                return Convoy;
            }

            string s = "";
            if ( UsableConvoy.Payload is string )
            {
                s += UsableConvoy.Payload + "\n";
            }
            else
            {
                IEnumerable<IStorageFile> SrcFiles = UsableConvoy.Payload as IEnumerable<IStorageFile>;

                foreach ( IStorageFile ISF in SrcFiles )
                {
                    s += await ISF.ReadString() + "\n";
                }
            }

            IStorageFile tmp = await AppStorage.MkTemp();
            await tmp.WriteString( s );

            return new ProcConvoy( this, new IStorageFile[] { tmp } );
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcResult( this ) );
        }
    }
}
