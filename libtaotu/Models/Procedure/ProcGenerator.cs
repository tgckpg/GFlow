using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Logging;

namespace libtaotu.Models.Procedure
{
	using Controls;
	using Crawler;

	class ProcGenerator : Procedure
	{
		public string EntryPoint { get; set; }

		public ObservableHashSet<string> Urls { get; private set; }

		public bool Incoming { get; set; }
		public string Prefix { get; set; }

		public ObservableCollection<ProcFind.RegItem> NextIfs { get; set; }
		public ObservableCollection<ProcFind.RegItem> StopIfs { get; set; }

		private bool _FirstStopSkip = false;
		public bool FirstStopSkip
		{
			get { return _FirstStopSkip; }
			set
			{
				_FirstStopSkip = value;
				NotifyChanged( "FirstStopSkip" );
			}
		}

		private bool _DiscardUnmatched = false;
		public bool DiscardUnmatched
		{
			get { return _DiscardUnmatched; }
			set
			{
				_DiscardUnmatched = value;
				NotifyChanged( "DiscardUnmatched" );
			}
		}

		private bool FirstStopped = false;

		protected override Color BgColor { get { return Colors.OrangeRed; } }

		public ProcGenerator()
			:base( ProcType.GENERATOR )
		{
			NextIfs = new ObservableCollection<ProcFind.RegItem>();
			StopIfs = new ObservableCollection<ProcFind.RegItem>();
			Urls = new ObservableHashSet<string>();
		}

		public override async Task<ProcConvoy> Run( ProcConvoy Convoy )
		{
			await base.Run( Convoy );

			string LoadUrl = null;
			FirstStopped = false;

			if ( Incoming )
			{
				ProcManager.PanelMessage( this, Res.RSTR( "IncomingCheck" ), LogType.INFO );

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
						LoadUrl = UsableConvoy.Payload as string;
					}
					else
					{
						LoadUrl = ( UsableConvoy.Payload as IEnumerable<string> ).FirstOrDefault();
					}

					if( !string.IsNullOrEmpty( LoadUrl ) )
					{
						LoadUrl = WebUtility.HtmlDecode( LoadUrl );
					}
				}
			}

			if ( string.IsNullOrEmpty( LoadUrl ) ) LoadUrl = EntryPoint;

			if( string.IsNullOrEmpty( LoadUrl ) )
			{
				ProcManager.PanelMessage( this, Res.RSTR( "NoEntryPoint" ), LogType.WARNING );
				return Convoy;
			}

			List<IStorageFile> ISFs = new List<IStorageFile>();
			bool Continue = true;
			Urls.Clear();
			NotifyChanged( "Urls" );

			while ( Continue )
			{
				if( string.IsNullOrEmpty( LoadUrl ) )
				{
					ProcManager.PanelMessage( this, Res.RSTR( "CannotCarrieOn" ), LogType.ERROR );
					break;
				}

				IStorageFile ISF = await ProceduralSpider.DownloadSource( LoadUrl );

				string Matchee = await ISF.ReadString();
				Continue = NextUrl( Matchee, out LoadUrl ) && !WillStop( Matchee );

				if ( Continue || !DiscardUnmatched )
				{
					ISFs.Add( ISF );
				}
			}

			return new ProcConvoy( this, ISFs );
		}

		private bool NextUrl( string v, out string loadUrl )
		{
			loadUrl = null;
			bool Continue = false;
			foreach( ProcFind.RegItem R in NextIfs )
			{
				if ( !R.Enabled ) continue;

				MatchCollection matches = R.RegExObj.Matches( v );
				foreach( Match match in matches )
				{
					string formatted = string.Format(
						R.Format
						, match.Groups
							.Cast<Group>()
							.Select( g => g.Value )
							.ToArray()
					);

					formatted = WebUtility.HtmlDecode( formatted );
					if ( Urls.Contains( formatted ) ) continue;

					Continue = true;
					if ( string.IsNullOrEmpty( formatted ) ) continue;

					Urls.Add( formatted );
					NotifyChanged( "Urls" );

					loadUrl = formatted;
				}

			}

			return Continue;
		}

		private bool WillStop( string v )
		{
			foreach( ProcFind.RegItem Reg in StopIfs )
			{
				if ( !Reg.Enabled ) continue;

				if ( Reg.RegExObj.IsMatch( v ) )
				{
					if( FirstStopSkip && !FirstStopped )
					{
						FirstStopped = true;
						return false;
					}

					ProcManager.PanelMessage( this, Res.RSTR( "MatchedStop", Reg.Pattern ), LogType.INFO );
					return true;
				}
			}

			return false;
		}

		public override void ReadParam( XParameter Param )
		{
			base.ReadParam( Param );

			EntryPoint = Param.GetValue( "EntryPoint" );
			Incoming = Param.GetBool( "Incoming" );
			FirstStopSkip = Param.GetBool( "FirstStopSkip" );
			DiscardUnmatched = Param.GetBool( "DiscardUnmatched" );

			XParameter NextParams = Param.Parameter( "NextIfs" );
			XParameter[] RegParams = NextParams.Parameters( "i" );
			foreach ( XParameter RegParam in RegParams )
			{
				NextIfs.Add( new ProcFind.RegItem( RegParam ) );
			}

			XParameter StopParams = Param.Parameter( "StopIfs" );
			RegParams = StopParams.Parameters( "i" );
			foreach ( XParameter RegParam in RegParams )
			{
				StopIfs.Add( new ProcFind.RegItem( RegParam ) );
			}
		}

		public override XParameter ToXParam()
		{
			XParameter Param = base.ToXParam();

			Param.SetValue( new XKey[] {
				new XKey( "EntryPoint", EntryPoint )
				, new XKey( "Incoming", Incoming )
				, new XKey( "FirstStopSkip", FirstStopSkip )
				, new XKey( "DiscardUnmatched", DiscardUnmatched )
			} );

			int i = 0;

			XParameter NextParams = new XParameter( "NextIfs" );
			foreach( ProcFind.RegItem R in NextIfs )
			{
				XParameter RegParam = R.ToXParam();
				RegParam.Id += i;
				RegParam.SetValue( new XKey( "i", i++ ) );

				NextParams.SetParameter( RegParam );
			}

			XParameter StopParams = new XParameter( "StopIfs" );
			foreach( ProcFind.RegItem R in StopIfs )
			{
				XParameter RegParam = R.ToXParam();
				RegParam.Id += i;
				RegParam.SetValue( new XKey( "i", i++ ) );

				StopParams.SetParameter( RegParam );
			}

			Param.SetParameter( NextParams );
			Param.SetParameter( StopParams );

			return Param;
		}

		public override async Task Edit()
		{
			await Popups.ShowDialog( new Dialogs.EditProcGenerator( this ) );
		}
	}
}