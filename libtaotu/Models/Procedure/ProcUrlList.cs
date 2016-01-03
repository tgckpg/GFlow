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
            await Popups.ShowDialog( new Dialogs.EditProcUrlList( this ) );
        }

        public async Task<IStorageFile> DownloadSource( string url )
        {
            TaskCompletionSource<IStorageFile> TCS = new TaskCompletionSource<IStorageFile>();
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
                catch( Exception ex )
                {
                    Logger.Log( ID, ex.Message, LogType.ERROR );
                    TCS.SetResult( null );
                }
            };

            Request.OpenAsync();

            return await TCS.Task;
        }
    }
}
