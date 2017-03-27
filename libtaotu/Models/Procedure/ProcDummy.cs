using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libtaotu.Models.Procedure
{
	/// <summary>
	/// A Dummy procedure designated for Test Running
	/// </summary>
	class ProcDummy : Procedure
	{
		public ProcDummy() :base( ProcType.DUMMY ) { }
		public ProcDummy( ProcType PType ) : base( ProcType.DUMMY | PType ) { }

		public override Task Edit()
		{
			throw new NotImplementedException();
		}
	}
}