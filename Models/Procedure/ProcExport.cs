using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Models.Procedure
{
	using Interfaces;
	class ProcExport : Procedure
	{
		public static readonly string ID = typeof( ProcExport ).Name;

		// public override Type PropertyPage => typeof( Dialogs.EditProcEncoding );

		public ProcExport()
			:base( ProcType.GENERIC )
		{
			RawName = "EXPORT";
		}

		public override Task<ProcConvoy> Run( ICrawler Crawler, ProcConvoy Convoy )
		{
			return Task.FromResult( Convoy );
		}

	}
}