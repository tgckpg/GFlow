using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.Messaging;

namespace libtaotu.Dialogs
{
    using Models.Procedure;
    using Pages;
    using Resources;

    sealed partial class EditProcEncoding : ContentDialog
    {
        public static readonly string ID = typeof( EditProcFind ).Name;

        private ProcEncoding EditTarget;
        private SortedDictionary<string, int> SupportedCodePages = new SortedDictionary<string, int>();

        private EditProcEncoding()
        {
            this.InitializeComponent();
            SetTemplate();
        }

        public EditProcEncoding( ProcEncoding EditTarget )
            :this()
        {
            this.EditTarget = EditTarget;
            MessageBus.OnDelivery += MessageBus_OnDelivery;
        }

        ~EditProcEncoding() { Dispose(); }

        public void Dispose()
        {
            MessageBus.OnDelivery -= MessageBus_OnDelivery;
        }

        private void SetTemplate()
        {
            StringResources stx = new StringResources( "/libtaotu/Message" );
            PrimaryButtonText = stx.Str( "OK" );

            int[] KnownCodePages = new int[] {
                37, 437, 500, 708, 720, 737, 775, 850, 852, 855, 857, 858, 860, 861, 862, 863, 864
                , 865, 866, 869, 870, 874, 875, 932, 936, 949, 950, 1026, 1047, 1140, 1141, 1142, 1143
                , 1144, 1145, 1146, 1147, 1148, 1149, 1200, 1201, 1250, 1251, 1252, 1253, 1254, 1255
                , 1256, 1257, 1258, 1361, 10000, 10001, 10002, 10003, 10004, 10005, 10006, 10007, 10008
                , 10010, 10017, 10021, 10029, 10079, 10081, 10082, 12000, 12001, 20000, 20001, 20002
                , 20003, 20004, 20005, 20105, 20106, 20107, 20108, 20127, 20261, 20269, 20273, 20277
                , 20278, 20280, 20284, 20285, 20290, 20297, 20420, 20423, 20424, 20833, 20838, 20866
                , 20871, 20880, 20905, 20924, 20932, 20936, 20949, 21025, 21866, 28591, 28592, 28593
                , 28594, 28595, 28596, 28597, 28598, 28599, 28603, 28605, 29001, 38598, 50220, 50221
                , 50222, 50225, 50227, 51932, 51936, 51949, 52936, 54936, 57002, 57003, 57004, 57005
                , 57006, 57007, 57008, 57009, 57010, 57011, 65000, 65001 };

            Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );

            foreach ( int CodePage in KnownCodePages )
            {
                try
                {
                    Encoding Enc = Encoding.GetEncoding( CodePage );
                    SupportedCodePages.Add( Enc.EncodingName, Enc.CodePage );
                }
                catch ( Exception ) { }
            }

            Encodings.ItemsSource = SupportedCodePages;
        }

        private void Encodings_Loaded( object sender, RoutedEventArgs e )
        {
            try
            {
                Encodings.SelectedValue = EditTarget.CodePage;
            }
            catch ( Exception )
            {
                Encodings.SelectedValue = Encoding.UTF8.CodePage;
            }
        }

        private void ChangeEncoding( object sender, SelectionChangedEventArgs e )
        {
            EditTarget.CodePage = ( ( KeyValuePair<string, int> ) e.AddedItems[ 0 ] ).Value;
        }

        private void RunTilHere( object sender, RoutedEventArgs e )
        {
            TestRunning.IsActive = true;
            MessageBus.SendUI( typeof( ProceduresPanel ), "RUN", EditTarget );
        }

        private void MessageBus_OnDelivery( Message Mesg )
        {
            ProcConvoy Convoy = Mesg.Payload as ProcConvoy;
            if ( Mesg.Content == "RUN_RESULT"
                && Convoy != null
                && Convoy.Dispatcher == EditTarget )
            {
                TestRunning.IsActive = false;

                IEnumerable<IStorageFile> ISF = Convoy.Payload as IEnumerable<IStorageFile>;
                if( ISF != null )
                {
                    Preview.Navigate( Shared.SourceView, ISF.First() );
                }
            }
        }
    }
}