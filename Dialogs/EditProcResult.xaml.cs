using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Messaging;

namespace libtaotu.Dialogs
{
    using Models.Procedure;
    using Pages;
    using Resources;

    sealed partial class EditProcResult : ContentDialog
    {
        private ProcResult EditTarget;

        private IStorageFile TestResult;

        private EditProcResult()
        {
            this.InitializeComponent();
            SetTemplate();
        }

        private void SetTemplate()
        {
            StringResources stx = new StringResources( "/libtaotu/Message" );
            PrimaryButtonText = stx.Str( "OK" );

            MessageBus.OnDelivery += MessageBus_OnDelivery;
        }

        ~EditProcResult() { Dispose(); }

        public void Dispose()
        {
            MessageBus.OnDelivery -= MessageBus_OnDelivery;
        }

        public EditProcResult( ProcResult EditTarget )
            :this()
        {
            this.EditTarget = EditTarget;
        }

        private void RunTilHere( object sender, RoutedEventArgs e )
        {
            TestRunning.IsActive = true;
            MessageBus.SendUI( new Message( typeof( ProceduresPanel ), "RUN", EditTarget ) );
        }

        private async void SaveResult( object sender, RoutedEventArgs e )
        {
            if( TestResult != null )
            {
                IStorageFile SaveTarget = await AppStorage.SaveFileAsync( "Test Result", new string[] { ".txt", ".html" } );
                if ( SaveTarget == null ) return;

                await TestResult.CopyAndReplaceAsync( SaveTarget );
            }
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
                    Preview.Navigate( Shared.SourceView, TestResult = ISF.First() );
                }
            }
        }
    }
}
