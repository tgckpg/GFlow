using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libtaotu.Models.Procedure
{
    class ProcDownload : Procedure
    {
        public ProcDownload()
        {
            Type = ProcType.DOWNLOAD;
        }

        public override Task Edit()
        {
            throw new NotImplementedException();
        }
    }
}
