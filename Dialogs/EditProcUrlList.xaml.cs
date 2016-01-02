using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using libtaotu.Models.Procedure;

namespace libtaotu.Dialogs
{
    sealed partial class EditProcUrlList : ContentDialog
    {
        private ProcUrlList EditTarget;
        private EditProcUrlList()
        {
            InitializeComponent();
        }

        public EditProcUrlList( ProcUrlList procUrlList )
            :this()
        {
            EditTarget = procUrlList;
            UrlList.ItemsSource = EditTarget.Urls;
            UrlInput.KeyDown += UrlInput_KeyDown;
        }

        private void UrlInput_KeyDown( object sender, KeyRoutedEventArgs e )
        {
            if( e.Key == Windows.System.VirtualKey.Enter )
            {
                TryAddUrl();
            }
        }

        private void ContentDialog_PrimaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
        }

        private void AddUrl( object sender, RoutedEventArgs e )
        {
            TryAddUrl();
        }

        private void RemoveUrl( object sender, RoutedEventArgs e )
        {
            Button B = sender as Button;
            string s = B.DataContext as string;
            EditTarget.Urls.Remove( s );

            UrlList.ItemsSource = null;
            UrlList.ItemsSource = EditTarget.Urls;
        }

        private void TryAddUrl()
        {
            string url = UrlInput.Text.Trim();
            if ( string.IsNullOrEmpty( url ) ) return;

            try
            {
                Uri u = new Uri( url );
                switch( u.Scheme )
                {
                    case "http":
                    case "https":
                        break;
                    default:
                        throw new InvalidDataException( "Url" );
                }

                EditTarget.Urls.Add( url );

                UrlInput.Text = "";
                UrlList.ItemsSource = null;

                UrlList.ItemsSource = EditTarget.Urls;
            }
            catch( Exception )
            {
            }
        }
    }
}
