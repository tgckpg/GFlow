using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Models.Procedure
{
	class ProcConvoy
	{
		public Procedure Dispatcher { get; private set; }

		public object Payload { get; private set; }

		public ProcConvoy( Procedure Dispatcher, object Payload )
		{
			this.Dispatcher = Dispatcher;
			this.Payload = Payload;
		}
	}
}
