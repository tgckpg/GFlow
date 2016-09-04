using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;

namespace libtaotu.Crawler
{
    using Controls;
    using Models.Procedure;
    using Resources;

    sealed class ProceduralSpider
    {
        public static readonly string ID = typeof( ProceduralSpider ).Name;

        private IEnumerable<Procedure> ProcList;

        public ProceduralSpider( IEnumerable<Procedure> ProcList )
        {
            this.ProcList = ProcList;
        }

        public async Task<ProcConvoy> Crawl( ProcConvoy Convoy = null )
        {
            if ( ProcList.Count() == 0 )
            {
                ProcManager.PanelMessage( ID, () => Res.RSTR( "EmptyCrawling" ), LogType.INFO );
                return Convoy;
            }

            ProcConvoy Conveying = Convoy;

            foreach ( Procedure Proc in ProcList )
            {
                ProcManager.PanelMessage( ID, Res.SSTR( "Running", Proc.Name ), LogType.INFO );

                try
                {
                    Proc.Running = true;
                    ProcConvoy Received = await Proc.Run( Conveying );
                    Conveying = Received;
                    Proc.Running = false;
                }
                catch ( Exception ex )
                {
                    ProcManager.PanelMessage( ID, () => Res.RSTR( "Faulted", Proc.Name, ex.Message ), LogType.ERROR );
                    Proc.Running = false;
                    Proc.Faulted = true;
                    Conveying = null;
                    break;
                }
            }

            ProcManager.PanelMessage( ID, () => Res.RSTR( "RunComplete" ), LogType.INFO );
            return Conveying;
        }

        public static async Task<IStorageFile> DownloadSource( string url )
        {
            ProcManager.PanelMessage( ID, Res.SSTR( "Download", url ), LogType.INFO );

            TaskCompletionSource<IStorageFile> TCS = new TaskCompletionSource<IStorageFile>();

            try
            {
                HttpRequest Request = Shared.CreateRequest( new Uri( url ) );

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
                        ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                        TCS.SetResult( null );
                    }
                };

                Request.OpenAsync();
            }
            catch( UriFormatException )
            {
                ProcManager.PanelMessage( ID, Res.SSTR( "InvalidURL", url ), LogType.ERROR );
                TCS.TrySetResult( null );
            }
            catch ( Exception ex )
            {
                ProcManager.PanelMessage( ID, ex.Message, LogType.ERROR );
                TCS.TrySetResult( null );
            }
            return await TCS.Task;
        }

    }
}