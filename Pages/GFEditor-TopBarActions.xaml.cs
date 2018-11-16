﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Loaders;

namespace GFlow.Pages
{
	using Controls;

	public sealed partial class GFEditor : Page
	{
		IStorageFile CurrentFile;
		DispatcherTimer AutoBackupTimer = new DispatcherTimer();

		private void StartAutoBackup()
		{
			AutoBackupTimer.Interval = TimeSpan.FromSeconds( 60 );
			AutoBackupTimer.Tick += ( s, e ) => Backup();
			AutoBackupTimer.Start();
		}

		private async void SaveBtn_Click( object sender, RoutedEventArgs e ) => await Save();

		private async void SaveAsBtn_Click( object sender, RoutedEventArgs e )
		{
			IStorageFile ISF = await AppStorage.SaveFileAsync( "GFlow Control Graph", new string[] { ".xml" } );
			if ( ISF == null ) return;

			using ( Stream s = await ISF.OpenStreamForWriteAsync() )
				Unsafe_WriteDrawboard( s );
		}

		private async void NewFile_Click( object sender, RoutedEventArgs e )
		{
			if ( await SaveChanges() == null )
				return;

			DBoard?.Dispose();
			CurrentFile = null;
			DBoard = new GFDrawBoard( DrawBoard );
			AutoBackupTimer.Stop();
			AutoBackupTimer.Start();
		}

		private async void Open_Click( object sender, RoutedEventArgs e )
		{
			if ( await SaveChanges() == null )
				return;

			IStorageFile ISF = await AppStorage.OpenFileAsync( ".xml" );
			if ( ISF == null ) return;

			using ( Stream s = await ISF.OpenStreamForReadAsync() )
				Unsafe_ReadDrawboard( s );

			FileName.Text = ISF.Name;
			CurrentFile = ISF;
			await CheckForBackup();

			ResetAutoBackupTimer();
		}

		private async Task<bool?> SaveChanges()
		{
			if ( !await FilesChanged() )
				return true;

			bool? Confirmed = null;

			StringResources stx = StringResources.Load( "/GFlow/Message" );
			MessageDialog MsgBox = new MessageDialog( string.Format( stx.Str( "SaveChanges" ), FileName.Text ) );
			MsgBox.Commands.Add( new UICommand( stx.Str( "Yes" ), x => Confirmed = true ) );
			MsgBox.Commands.Add( new UICommand( stx.Str( "No" ), x => Confirmed = false ) );
			MsgBox.Commands.Add( new UICommand( stx.Str( "Cancel" ) ) );

			await Popups.ShowDialog( MsgBox );

			if ( Confirmed == true && !await Save() )
				return null;

			DropBackup();
			return Confirmed;
		}

		private async Task<bool> Save()
		{
			if ( CurrentFile == null )
			{
				CurrentFile = await AppStorage.SaveFileAsync( "GFlow Control Graph", new string[] { ".xml" } );
				if ( CurrentFile == null )
				{
					return false;
				}
			}

			using ( Stream s = await CurrentFile.OpenStreamForWriteAsync() )
				Unsafe_WriteDrawboard( s );

			FileName.Text = CurrentFile.Name;

			DropBackup();
			ResetAutoBackupTimer();
			return true;
		}

		private async Task<bool> FilesChanged()
		{
			if ( CurrentFile == null )
			{
				return DBoard.Children.Any();
			}

			using ( Stream s = await CurrentFile.OpenStreamForReadAsync() )
			using ( MemoryStream ms = new MemoryStream() )
			{
				Unsafe_WriteDrawboard( ms );
				ms.Position = 0;
				return !Unsafe_StreamEqual( ms, s );
			}
		}

		private async void Backup()
		{
			if ( !await FilesChanged() )
				return;

			IStorageFile BackupFile = await AppStorage.GetTemp( "GFlow", DBoard.BoardId.ToString() + ".xml", true );
			using ( Stream s = await BackupFile.OpenStreamForWriteAsync() )
				Unsafe_WriteDrawboard( s );
		}

		private void ResetAutoBackupTimer()
		{
			AutoBackupTimer.Stop();
			AutoBackupTimer.Start();
		}

		private async void DropBackup()
		{
			IStorageFile BackupFile = await AppStorage.GetTemp( "GFlow", DBoard.BoardId.ToString() + ".xml" );
			if ( BackupFile == null )
				return;

			await BackupFile.DeleteAsync();
		}

		private async Task CheckForBackup()
		{
			if ( CurrentFile == null )
				return;

			IStorageFile BackupFile = await AppStorage.GetTemp( "GFlow", DBoard.BoardId.ToString() + ".xml" );
			if ( BackupFile == null )
				return;

			DateTimeOffset LMBackup = ( await BackupFile.GetBasicPropertiesAsync() ).DateModified;
			DateTimeOffset LMCurrent = ( await CurrentFile.GetBasicPropertiesAsync() ).DateModified;

			if ( LMCurrent < LMBackup )
			{
				using ( Stream Curr = await CurrentFile.OpenStreamForReadAsync() )
				using ( Stream Back = await BackupFile.OpenStreamForReadAsync() )
				{
					if ( Unsafe_StreamEqual( Curr, Back ) )
					{
						await BackupFile.DeleteAsync();
						return;
					}
				}

				bool RestoreBackup = false;

				StringResources stx = StringResources.Load( "/GFlow/Message" );
				MessageDialog MsgBox = new MessageDialog( stx.Str( "RestoreBackup" ) );
				MsgBox.Commands.Add( new UICommand( stx.Str( "Yes" ), x => RestoreBackup = true ) );
				MsgBox.Commands.Add( new UICommand( stx.Str( "No" ) ) );

				await Popups.ShowDialog( MsgBox );
				if ( !RestoreBackup )
				{
					await BackupFile.DeleteAsync();
					return;
				}

				using ( Stream s = await BackupFile.OpenStreamForReadAsync() )
					Unsafe_ReadDrawboard( s );
			}
		}

		private bool Unsafe_StreamEqual( Stream A, Stream B )
		{
			SHA1 Hasher = SHA1.Create();
			return Hasher.ComputeHash( A ).SequenceEqual( Hasher.ComputeHash( B ) );
		}

		private void Unsafe_ReadDrawboard( Stream s )
		{
			if ( DBoard != null )
			{
				DBoard.Dispose();
			}

			if ( 0 < s.Length )
			{
				DataContractSerializer DCS = new DataContractSerializer( typeof( GFDrawBoard ) );
				DBoard = DCS.ReadObject( s ) as GFDrawBoard;
				DBoard.Find<GFProcedure>().ExecEach( ( Action<GFProcedure> ) BindGFPEvents );
				DBoard.SetStage( DrawBoard );
			}
			else
			{
				DBoard = new GFDrawBoard( DrawBoard );
			}
		}

		private void Unsafe_WriteDrawboard( Stream s )
		{
			DataContractSerializerSettings Conf = new DataContractSerializerSettings();
			Conf.PreserveObjectReferences = true;

			DataContractSerializer DCS = new DataContractSerializer( typeof( GFDrawBoard ), Conf );
			DCS.WriteObject( s, DBoard );

			s.SetLength( s.Position );
		}

	}
}