using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;

namespace libtaotu.Models.Procedure
{
    using Controls;
    using Crawler;

    class ProcUrlList : Procedure
    {
        public static readonly string ID = typeof( ProcUrlList ).Name;

        public HashSet<string> Urls { get; private set; }
        public bool Incoming { get; set; }
        public string Prefix { get; set; }

        public ProcUrlList()
            : base( ProcType.URLLIST )
        {
            Urls = new HashSet<string>();
            Prefix = "";
        }

        public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            await base.Run( Convoy );

            HashSet<string> ConvoyUrls = null;

            if ( Incoming )
            {
                ProcManager.PanelMessage( this, "Checking Incoming Urls", LogType.INFO );

                ProcConvoy UsableConvoy = ProcManager.TracePackage(
                    Convoy, ( P, C ) =>
                    {
                        return C.Payload is IEnumerable<string> || C.Payload is string;
                    }
                );

                if ( UsableConvoy != null )
                {
                    if ( UsableConvoy.Payload is string )
                    {
                        ConvoyUrls = new HashSet<string>();
                        ConvoyUrls.Add( UsableConvoy.Payload as string );
                    }
                    else
                    {
                        ConvoyUrls = new HashSet<string>( UsableConvoy.Payload as IEnumerable<string> );
                    }
                }
            }

            if ( ConvoyUrls == null && Urls.Count == 0 )
            {
                ProcManager.PanelMessage( this, "Empty list, did you forget to add urls?", LogType.WARNING );
            }

            List<IStorageFile> ISF = new List<IStorageFile>();

            foreach ( string u in Urls )
            {
                ISF.Add( await ProceduralSpider.DownloadSource( Prefix + u ) );
            }

            if ( ConvoyUrls != null )
            {
                foreach ( string u in ConvoyUrls )
                {
                    ISF.Add( await ProceduralSpider.DownloadSource( Prefix + u ) );
                }
            }

            return new ProcConvoy( this, ISF );
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcUrlList( this ) );
        }

        public override void ReadParam( XParameter Param )
        {
            base.ReadParam( Param );

            Incoming = Param.GetBool( "Incoming" );
            Prefix = Param.GetValue( "Prefix" );

            XParameter[] Params = Param.GetParametersWithKey( "url" );
            foreach( XParameter P in Params )
            {
                Urls.Add( P.GetValue( "url" ) );
            }
        }

        public override XParameter ToXParam()
        {
            XParameter Param = base.ToXParam();
            int i = 0;

            Param.SetValue( new XKey[] {
                new XKey( "Incoming", Incoming )
                , new XKey( "Prefix", Prefix )
            } );

            foreach ( string url in Urls )
            {
                Param.SetParameter( ( i++ ) + "", new XKey( "url", url ) );
            }

            return Param;
        }
    }
}
