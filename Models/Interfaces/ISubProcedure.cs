using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GFlow.Controls;
using GFlow.Models.Procedure;

namespace GFlow.Models.Interfaces
{
	interface ISubProcedure
	{
		ProcManager SubProcedures { get; set; }
		void SubEditComplete();
	}
}
