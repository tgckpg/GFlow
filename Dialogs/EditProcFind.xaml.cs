using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

using libtaotu.Models.Procedure;
using libtaotu.Resources;

namespace libtaotu.Dialogs
{
    sealed partial class EditProcFind : ContentDialog
    {
        public static readonly string ID = typeof( EditProcFind ).Name;

        private IStorageFile TestContent;

        private ProcFind EditTarget;

        private EditProcFind()
        {
            this.InitializeComponent();
            SetTemplate();
        }

        private void SetTemplate()
        {

        }

        public EditProcFind( ProcFind EditTarget )
            :this()
        {
            this.EditTarget = EditTarget;

            if( EditTarget.RegexPairs.Count == 0 )
            {
                EditTarget.RegexPairs.Add( new ProcFind.RegItem() );
            }

            RegexControl.DataContext = EditTarget;
        }

        private async void PreviewProcess( object sender, RoutedEventArgs e )
        {
            string Url = TestLink.Text.Trim();
            if ( string.IsNullOrEmpty( Url ) ) return;

            try
            {
                TestContent = await new ProcUrlList().DownloadSource( Url );
            }
            catch( Exception ex )
            {
                Logger.Log( ID, ex.Message, LogType.INFO );
                return;
            }

            UpdateTestSubject();
        }

        private void AddRegex( object sender, RoutedEventArgs e )
        {
            EditTarget.RegexPairs.Add( new ProcFind.RegItem() );
        }

        private void SetPattern( object sender, RoutedEventArgs e )
        {
            TextBox Input = sender as TextBox;
            ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
            Item.Pattern = Input.Text;

            EditTarget.ValidateRegex( Item );
        }

        private void SetFormat( object sender, RoutedEventArgs e )
        {
            TextBox Input = sender as TextBox;
            ProcFind.RegItem Item = Input.DataContext as ProcFind.RegItem;
            Item.Format = Input.Text;

            EditTarget.ValidateRegex( Item );
        }

        private void RemoveRegex( object sender, RoutedEventArgs e )
        {
            Button B = sender as Button;
            EditTarget.RemoveRegex( B.DataContext as ProcFind.RegItem );

            UpdateTestSubject();
        }

        private void ApplyRegex( object sender, RoutedEventArgs e )
        {
            Button B = sender as Button;
            ProcFind.RegItem Item = B.DataContext as ProcFind.RegItem;

            Item.Enabled = !Item.Enabled;
            ToggleIcon( Item.Enabled, sender as Button );

            UpdateTestSubject();
        }

        private void ToggleIcon( bool v, Button button )
        {
            IconBase Icon = button.ChildAt<IconBase>( 0, 0, 0 );
            Icon.Foreground = new SolidColorBrush(
                v ? Windows.UI.Colors.DodgerBlue : Windows.UI.Colors.White
            );
        }

        private async void UpdateTestSubject()
        {
            if ( TestContent == null ) return;
            IStorageFile ISF = await EditTarget.FilterContent( TestContent );

            Frame.Navigate( Shared.SourceView, ISF );
            FrameContainer.Visibility = Visibility.Visible;
        }
    }
}