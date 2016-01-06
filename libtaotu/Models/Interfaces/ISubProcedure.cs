using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libtaotu.Controls;
using libtaotu.Models.Procedure;

namespace libtaotu.Models.Interfaces
{
    interface ISubProcedure
    {
        ProcManager SubProcedures { get; set; }
        void SubEditComplete();
    }
}
