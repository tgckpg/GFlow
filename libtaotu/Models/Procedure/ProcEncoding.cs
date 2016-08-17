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

    class ProcEncoding : Procedure
    {
        public static readonly string ID = typeof( ProcEncoding ).Name;

        public int CodePage { get; set; }

        protected override IconBase Icon { get { return new IconRetract() { AutoScale = true }; } }
        protected override Color BgColor { get { return Colors.MidnightBlue; } }

        public ProcEncoding()
            : base( ProcType.ENCODING )
        {
            CodePage = Encoding.UTF8.CodePage;
        }

        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );

            // Search for usable convoy
            ProcConvoy UsableConvoy;
            if ( !TryGetConvoy( out UsableConvoy, ( P, C ) =>
            {
                return C.Payload is IEnumerable<IStorageFile>
                || C.Payload is string;
            }
            ) ) return Convoy;

            try
            {
                IEnumerable<IStorageFile> ISFs = ( IEnumerable<IStorageFile> ) Convoy.Payload;

                Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
                Encoding Enc = Encoding.GetEncoding( CodePage );
                ProcManager.PanelMessage( ID, Res.RSTR( "ReadEncoding" ) + ": " + Enc.EncodingName, LogType.INFO );

                foreach ( IStorageFile ISF in ISFs )
                {
                    ProcManager.PanelMessage( ID, Res.RSTR( "Converting Encoding" ), LogType.INFO );
                    await ISF.WriteString( await ISF.ReadString( Enc ) );
                }
            }
            catch ( Exception ex )
            {
                ProcManager.PanelMessage( ID, Res.RSTR( "EncodingFalied" ) + ": " + ex.Message, LogType.INFO );
            }

            return new ProcConvoy( this, Convoy.Payload );
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcEncoding( this ) );
        }

        public override void ReadParam( XParameter Param )
        {
            base.ReadParam( Param );

            XParameter[] RegParams = Param.Parameters( "i" );
            CodePage = Param.GetSaveInt( "CodePage" );
        }

        public override XParameter ToXParam()
        {
            XParameter Param = base.ToXParam();

            Param.SetValue( new XKey[] {
                new XKey( "CodePage", CodePage )
            } );

            return Param;
        }
    }
}