using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.Helpers;
using Net.Astropenguin.IO;
using Net.Astropenguin.Linq;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace GFlow.Models.Procedure
{
	using Controls;
	using Models.Interfaces;

	enum RunMode { FEEDBACK = 1, INPUT = 2, SOURCE_AVAIL = 4, DEFINE = 8, OUTPUT = 16 }

	class ProcParameter : Procedure
	{
		public static readonly string ID = typeof( ProcParameter ).Name;

		public RunMode Mode { get; set; }

		public bool Incoming { get; set; }
		public string TemplateStr { get; set; }

		public string Caption { get; set; }

		public ObservableCollection<ParamDef> ParamDefs { get; private set; }

		public string RawModeName { get; private set; }
		public string ModeName { get { return ProcStrRes.Str( RawModeName ); } }

		public override Type PropertyPage => typeof( Dialogs.EditProcParam );
		protected override IconBase Icon { get { return new IconInfo() { AutoScale = true }; } }
		protected override Color BgColor { get { return Colors.RoyalBlue; } }

		public ProcParameter()
			: base( ProcType.PARAMETER )
		{
			ParamDefs = new ObservableCollection<ParamDef>();
			TemplateStr = "";

			SetMode( RunMode.DEFINE );
		}

		public void RemoveDef( ParamDef Item )
		{
			ParamDefs.Remove( Item );
			UpdateIndex();
			NotifyChanged( "ParamDefs" );
		}

		public void AddDef( ParamDef Item )
		{
			Item.Indexer = Indexer;
			ParamDefs.Add( Item );
			UpdateIndex();
			NotifyChanged( "ParamDefs" );
		}

		public void ToggleMode()
		{
			switch ( Mode )
			{
				case RunMode.DEFINE:
					SetMode( RunMode.SOURCE_AVAIL );
					break;
				case RunMode.SOURCE_AVAIL:
					SetMode( RunMode.INPUT );
					break;
				case RunMode.INPUT:
					SetMode( RunMode.FEEDBACK );
					break;
				case RunMode.FEEDBACK:
				default:
					SetMode( RunMode.DEFINE );
					break;
			}
		}

		public override async Task<ProcConvoy> Run( ICrawler Crawler, ProcConvoy Convoy )
		{
			await base.Run( Crawler, Convoy );

			ProcConvoy UsableConvoy;

			bool IsFeedRun = ( ProcManager.TracePackage( Convoy, ( P, C ) => ( P.Type & ProcType.FEED_RUN ) != 0 ) != null );

			Crawler.PLog( this, Res.RSTR( "RunMode", ModeName ), LogType.INFO );

			switch ( Mode )
			{
				case RunMode.FEEDBACK:
					if ( !IsFeedRun )
					{
						Crawler.PLog( this, Res.RSTR( "NotAFeedRun" ), LogType.INFO );
						return Convoy;
					}

					goto case RunMode.DEFINE;

				case RunMode.DEFINE:
					UsableConvoy = ProcManager.TracePackage( Convoy, ( P, C ) =>
						C.Payload is IEnumerable<IStorageFile>
						|| C.Payload is IEnumerable<string>
						|| C.Payload is IStorageFile
						|| C.Payload is string
					);

					if ( Incoming )
					{
						if ( UsableConvoy == null )
						{
							Crawler.PLog( this, Res.RSTR( "NoUsableConvoy" ), LogType.WARNING );
							return Convoy;
						}
						return await IncomingTemplates( Crawler, UsableConvoy );
					}
					else if ( UsableConvoy != null )
					{
						return await IncomingArguments( Crawler, UsableConvoy );
					}

					goto DEFAULT_PARAMS;

				/*** Belows only accepts incoming templates ***/
				case RunMode.INPUT:
					if( Worker.BackgroundOnly )
					{
						Crawler.PLog( this, Res.RSTR( "InputBackground" ), LogType.INFO );
						goto case RunMode.SOURCE_AVAIL;
					}

					if ( IsFeedRun )
					{
						Crawler.PLog( this, Res.RSTR( "InputFeedRunSkip" ), LogType.INFO );
						return Convoy;
					}

					ProcConvoy Con = ProcManager.TracePackage( Convoy, ( P, C ) => ( P.Type & ProcType.TEST_RUN ) != 0 );
					if ( Con != null )
					{
						Crawler.PLog( this, Res.RSTR( "TestRun_UseDefault" ), LogType.INFO );
						break;
					}

					Dialogs.InputProcParam InputDialog = null;
					await Worker.RunUITaskAsync( () =>
					{
						InputDialog = new Dialogs.InputProcParam( this );
						return Popups.ShowDialog( InputDialog );
					} );

					if ( InputDialog.Canceled )
					{
						throw new OperationCanceledException( Res.RSTR( "OperationCanceled" ) );
					}

					// Input sets the dafault values
					break;

				case RunMode.SOURCE_AVAIL:
					if ( !TryGetConvoy( out UsableConvoy, ( P, C ) => P is ProcParameter ) )
						return Convoy;

					ProcParameter Proc = ( ProcParameter ) UsableConvoy.Dispatcher;
					ParamDefs = Proc.ParamDefs;
					break;
			}

			if ( Incoming )
			{
				if ( !TryGetConvoy( out UsableConvoy, ( P, C ) =>
					C.Payload is IEnumerable<IStorageFile>
					|| C.Payload is IEnumerable<string>
					|| C.Payload is IStorageFile
					|| C.Payload is string
				) ) return Convoy;

				return await IncomingTemplates( Crawler, UsableConvoy );
			}

			DEFAULT_PARAMS:
			return new ProcConvoy( this, ApplyParams( Crawler ) );
		}

		public string ApplyParams( ICrawler Crawler, params string[] Args )
		{
			return FormatParams( Crawler, TemplateStr, Args );
		}

		public void SetDefaults( string[] Defaults )
		{
			int l = ParamDefs.Count;
			int m = Defaults.Length;

			for ( int i = 0; i < l && i < m; i++ )
			{
				ParamDefs[ i ].Default = Defaults[ i ];
			}
		}

		public void UpdateIndex()
		{
			foreach ( ParamDef P in ParamDefs ) P.Index = null;
		}

		public override async Task Edit()
		{
			// await Popups.ShowDialog( new Dialogs.EditProcParam( this ) );
		}

		private async Task<ProcConvoy> IncomingTemplates( ICrawler Crawler, ProcConvoy UsableConvoy )
		{
			if ( UsableConvoy.Payload is string )
			{
				return new ProcConvoy( this, FormatParams( Crawler, ( string ) UsableConvoy.Payload ) );
			}
			else if ( UsableConvoy.Payload is IStorageFile )
			{
				IStorageFile tmp = await AppStorage.MkTemp();
				await tmp.WriteString( FormatParams( Crawler, await ( ( IStorageFile ) UsableConvoy.Payload ).ReadString() ) );
				return new ProcConvoy( this, tmp );
			}
			else if ( UsableConvoy.Payload is IEnumerable<IStorageFile> )
			{
				IEnumerable<IStorageFile> Templates = ( IEnumerable<IStorageFile> ) UsableConvoy.Payload;
				List<IStorageFile> TemplatedStrs = new List<IStorageFile>();

				foreach ( IStorageFile Template in Templates )
				{
					IStorageFile tmp = await AppStorage.MkTemp();
					await tmp.WriteString( FormatParams( Crawler, await Template.ReadString() ) );
					TemplatedStrs.Add( tmp );
				}

				return new ProcConvoy( this, TemplatedStrs );
			}
			else
			{
				IEnumerable<string> Templates = ( IEnumerable<string> ) UsableConvoy.Payload;
				return new ProcConvoy( this, Templates.Remap( x => FormatParams( Crawler, x ) ) );
			}
		}

		private async Task<ProcConvoy> IncomingArguments( ICrawler Crawler, ProcConvoy UsableConvoy )
		{
			if ( UsableConvoy.Payload is string )
			{
				return new ProcConvoy( this, ApplyParams( Crawler, ( ( string ) UsableConvoy.Payload ).Split( new char[] { '\n' }, ParamDefs.Count ) ) );
			}
			else if ( UsableConvoy.Payload is IStorageFile )
			{
				IStorageFile tmp = await AppStorage.MkTemp();
				await tmp.WriteString( ApplyParams( Crawler, await ( ( IStorageFile ) UsableConvoy.Payload ).ReadLines( ParamDefs.Count ) ) );
				return new ProcConvoy( this, tmp );
			}
			else if ( UsableConvoy.Payload is IEnumerable<IStorageFile> )
			{
				IEnumerable<IStorageFile> Args = ( IEnumerable<IStorageFile> ) UsableConvoy.Payload;
				List<IStorageFile> TemplatedStrs = new List<IStorageFile>();

				foreach ( IStorageFile Arg in Args )
				{
					IStorageFile tmp = await AppStorage.MkTemp();
					await tmp.WriteString( ApplyParams( Crawler, await Arg.ReadLines( ParamDefs.Count ) ) );
					TemplatedStrs.Add( tmp );
				}

				return new ProcConvoy( this, TemplatedStrs );
			}
			else
			{
				IEnumerable<string> Args = ( IEnumerable<string> ) UsableConvoy.Payload;
				return new ProcConvoy( this, Args.Remap( x => ApplyParams( Crawler, x ) ) );
			}
		}

		private void SetMode( string name )
		{
			switch ( name )
			{
				case "FEEDBACK":
					SetMode( RunMode.FEEDBACK );
					break;
				case "INPUT":
					SetMode( RunMode.INPUT );
					break;
				case "SOURCE_AVAIL":
					SetMode( RunMode.SOURCE_AVAIL );
					break;
				case "DEFINE":
				default:
					SetMode( RunMode.DEFINE );
					break;
			}
		}

		private void SetMode( RunMode Mode )
		{
			this.Mode = Mode;
			RawModeName = Enum.GetName( typeof( RunMode ), Mode );
			NotifyChanged( "RawModeName", "ModeName" );
		}

		public override void ReadParam( XParameter Param )
		{
			base.ReadParam( Param );

			Incoming = Param.GetBool( "Incoming" );
			Caption = Param.GetValue( "Caption" );
			TemplateStr = Param.GetValue( "TemplateStr" );
			SetMode( Param.GetValue( "Mode" ) );

			SetParamDefs( Param );
		}

		public override XParameter ToXParam()
		{
			XParameter Param = base.ToXParam();

			Param.SetValue( new XKey[] {
				new XKey( "TemplateStr", TemplateStr )
				, new XKey( "Caption", Caption )
				, new XKey( "Incoming", Incoming )
				, new XKey( "Mode", RawModeName )
			} );

			AssignParamDefs( Param );
			return Param;
		}

		private string FormatParams( ICrawler Crawler, string Template, params string[] Args )
		{
			try
			{
				int l = ParamDefs.Count;
				int m = Args.Length;

				for ( int i = 0; i < l && i < m; i++ )
				{
					ParamDefs[ i ].Default = Args[ i ];
				}

				return string.Format( Template.Unescape(), ( string[] ) ParamDefs.Remap( x => x.Default ) );
			}
			catch ( Exception ex )
			{
				Crawler.PLog( this, ex.Message, LogType.INFO );
			}

			return "";
		}

		private int Indexer( ParamDef P )
		{
			return ParamDefs.IndexOf( P );
		}

		public static void StoreParams( ProcConvoy Convoy, XRegistry Settings )
		{
			ProcParameter Defs = ( ProcParameter ) ProcManager.TracePackage( Convoy, ( P, C ) => P is ProcParameter )?.Dispatcher;
			if ( Defs == null ) return;

			XParameter PDefs = new XParameter( "PPValues" );
			Defs.AssignParamDefs( PDefs );

			Settings.SetParameter( PDefs );
		}

		public static ProcConvoy RestoreParams( XRegistry Settings, object Payload = null )
		{
			XParameter PPParams = Settings.Parameter( "PPValues" );
			if ( PPParams == null ) return new ProcConvoy( new ProcPassThru(), Payload );

			ProcParameter Proc = new ProcParameter();
			Proc.SetParamDefs( Settings.Parameter( "PPValues" ) );

			return new ProcConvoy( Proc, Payload );
		}

		public static void DestroyParams( XRegistry Settings )
		{
			Settings.RemoveParameter( "PPValues" );
		}

		private void SetParamDefs( XParameter Param )
		{
			XParameter[] Params = Param.Parameters( "i" );
			foreach ( XParameter P in Params )
			{
				AddDef( new ParamDef( P.GetValue( "label" ), P.GetValue( "default" ) )
				{
					Indexer = this.Indexer
				} );
			}
		}

		private void AssignParamDefs( XParameter Param )
		{
			int i = 0;
			foreach ( ParamDef P in ParamDefs )
			{
				XParameter Def = P.ToXParam();
				Def.Id += i;
				Def.SetValue( new XKey( "i", i++ ) );
				Param.SetParameter( Def );
			}
		}


		public class ParamDef : ActiveData
		{
			public Func<ParamDef, int> Indexer = ( x ) => 0;
			public int RIndex { get { return Indexer( this ); } }
			public string Index
			{
				get { return "{" + Indexer( this ) + "}"; }
				set { NotifyChanged( "Index" ); }
			}

			private string _Label;
			public string Label
			{
				get { return _Label; }
				set { _Label = value; NotifyChanged( "Label" ); }
			}

			private string _Default;
			public string Default
			{
				get { return _Default; }
				set { _Default = value; NotifyChanged( "Default" ); }
			}

			public ParamDef( string Label, string Default )
			{
				_Label = Label;
				_Default = Default;
			}

			public XParameter ToXParam()
			{
				XParameter Def = new XParameter( "P" );
				Def.SetValue( new XKey[] {
					new XKey( "label", _Label )
					, new XKey( "default", _Default )
				} );

				return Def;
			}
		}

	}
}