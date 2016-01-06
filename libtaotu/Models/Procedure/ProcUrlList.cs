using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Loaders;

namespace libtaotu.Models.Procedure
{
    using Controls;

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
                ProcManager.PanelMessage( ID, "Checking Incoming Urls", LogType.INFO );

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
                ProcManager.PanelMessage( TypeName, "Empty list, did you forget to add urls?", LogType.WARNING );
            }

            List<IStorageFile> ISF = new List<IStorageFile>();

            foreach ( string u in Urls )
            {
                ISF.Add( await DownloadSource( Prefix + u ) );
            }

            if ( ConvoyUrls != null )
            {
                foreach ( string u in ConvoyUrls )
                {
                    ISF.Add( await DownloadSource( Prefix + u ) );
                }
            }

            return new ProcConvoy( this, ISF );
        }

        public override async Task Edit()
        {
            await Popups.ShowDialog( new Dialogs.EditProcUrlList( this ) );
        }

        public async Task<IStorageFile> DownloadSource( string url )
        {
            ProcManager.PanelMessage( TypeName, "Downloading: " + url, LogType.INFO );

            TaskCompletionSource<IStorageFile> TCS = new TaskCompletionSource<IStorageFile>();

            try
            {
                HttpRequest Request = new HttpRequest( new Uri( url ) );

                StorageFile SF = await AppStorage.MkTemp();
                Request.OnRequestComplete += async ( DRequestCompletedEventArgs DArgs ) =>
                {
                    try
                    {
                        IRandomAccessStream IRS = await SF.OpenAsync( FileAccessMode.ReadWrite );

                        try
                        {
                            await IRS.WriteAsync( DArgs.ResponseBytes.AsBuffer() );
                        }
                        catch ( Exception ex )
                        {
                            await IRS.WriteAsync( Encoding.UTF8.GetBytes( ex.Message ).AsBuffer() );
                        }

                        await IRS.FlushAsync();
                        IRS.Dispose();
                        TCS.SetResult( SF );
                    }
                    catch ( Exception ex )
                    {
                        ProcManager.PanelMessage( TypeName, ex.Message, LogType.ERROR );
                        TCS.SetResult( null );
                    }
                };

                Request.OpenAsync();
            }
            catch ( Exception ex )
            {
                ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                TCS.TrySetResult( null );
            }
            return await TCS.Task;
        }

        public override void ReadParam( XParameter Param )
        {
            Incoming = Param.GetBool( "Incoming" );
            Prefix = Param.GetValue( "Prefix" );

            XParameter[] Params = Param.GetParametersWithKey( "url" );
            foreach( XParameter P in Params )
            {
                Urls.Add( P.GetValue( "url" ) );
            }
        }

        public override XParameter ToXParem()
        {
            XParameter Param = new XParameter( RawName );
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
