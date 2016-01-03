using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

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
