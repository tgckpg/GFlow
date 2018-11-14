using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

using Net.Astropenguin.DataModel;
using Net.Astropenguin.IO;
using Net.Astropenguin.Loaders;
using Net.Astropenguin.Logging;
using Net.Astropenguin.UI.Icons;

namespace GFlow.Models.Procedure
{
	using Controls;
	using Models.Interfaces;

	internal enum ProcType
	{
		URLLIST = 1,
		GENERATOR = 2,
		FIND = 4,

		MARK = 8,
		EXTRACT = 16,
		LIST = 32,

		PARAMETER = 64,
		CHAKRA = 128,
		ENCODING = 256,
		RESULT = 512,

		DUMMY = 1024,
		INSTRUCTION = 2048,
		PASSTHRU = 4096,

		TEST_RUN = 8192,
		FEED_RUN = 16384,

		TRANSLATE = 32768,
	}

	abstract class Procedure : ActiveData, INamable
	{
		public ProcType Type { get; protected set; }
		public string RawName { get; protected set; }

		public ProcConvoy Convoy { get; protected set; }
		public bool Faulted { get; set; }

		virtual protected Color BgColor { get { return Colors.Gray; } }
		public Brush Background { get { return new SolidColorBrush( BgColor ); } }

		virtual protected IconBase Icon { get { return new IconAtomic() { AutoScale = true }; } }
		public IconBase BlockIcon { get { return Icon; } }

		private bool _running = false;
		public bool Running
		{
			get { return _running; }
			set { _running = value; NotifyChanged( "Running" ); }
		}

		private string _TypeName;
		public string TypeName
		{
			get
			{
				if ( string.IsNullOrEmpty( _TypeName ) )
					_TypeName = ProcStrRes.Str( RawName );

				return _TypeName;
			}
			set { _TypeName = value; }
		}

		private string _Name;
		public string Name
		{
			get
			{
				if ( string.IsNullOrEmpty( _Name ) )
					_Name = ProcStrRes.Str( RawName );

				return _Name;
			}
			set
			{
				_Name = value;
				NotifyChanged( "Name" );
			}
		}

		virtual public Type PropertyPage { get; }

		protected static StringResources ProcStrRes = StringResources.Load( "/GFlow/ProcItems" );

		public Procedure( ProcType P )
		{
			Type = P;
			RawName = Enum.GetName( typeof( ProcType ), P );
		}

		protected bool TryGetConvoy( out ProcConvoy Con, Func<Procedure, ProcConvoy, bool> Tester )
		{
			// Search for usable convoy
			Con = ProcManager.TracePackage( Convoy, Tester ); 

			if( Con == null )
			{
				ProcManager.PanelMessage( this, Res.RSTR( "NoUsablePayload" ), LogType.WARNING );
				Faulted = true;
				return false;
			}

			return true;
		}

		virtual public async Task<ProcConvoy> Run( ICrawler Crawler,  ProcConvoy Convoy )
		{
			return await Task.Run( () => this.Convoy = Convoy );
		}

		virtual public void ReadParam( XParameter Param )
		{
			string PName = Param.GetValue( "Name" );
			if ( PName != null ) Name = PName;
		}

		virtual public XParameter ToXParam()
		{
			XParameter Param = new XParameter( RawName );
			if( Name != TypeName )
			{
				Param.SetValue( new XKey( "Name", Name ) );
			}

			return Param;
		}
	}
}