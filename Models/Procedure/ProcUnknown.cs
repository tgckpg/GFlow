using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Models.Procedure
{
	class ProcUnknown : ProcDummy
	{
		public override Type PropertyPage => typeof( Dialogs.EditProcUnknown );

		public ProcUnknown( string RawName )
			:base()
		{
			this.RawName = RawName;
		}
	}
}