using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Models.Procedure
{
	/// <summary>
	/// This is a specialized procedure.
	/// Use it to pass dynamic convoy to sub procedure
	/// </summary>
	class ProcPassThru : Procedure
	{
		public ProcPassThru()
			: base( ProcType.PASSTHRU ) { }

		public ProcPassThru( ProcType PType )
			: base( ProcType.PASSTHRU | PType ) { }

		public ProcPassThru( ProcConvoy Convoy )
			: base( ProcType.PASSTHRU )
		{
			this.Convoy = Convoy;
		}

		public ProcPassThru( ProcConvoy Convoy, ProcType PType )
			: base( ProcType.PASSTHRU | PType )
		{
			this.Convoy = Convoy;
		}

		public override Task Edit()
		{
			throw new InvalidOperationException();
		}

	}
}