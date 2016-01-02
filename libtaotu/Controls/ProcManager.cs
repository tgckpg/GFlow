using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.UI.Xaml.Controls;

namespace libtaotu.Controls
{
    using Models.Procedure;
    class ProcManager
    {
        public ObservableCollection<Procedure> ProcList { get; private set; }

        public ProcManager()
        {
            ProcList = new ObservableCollection<Procedure>();
        }

        public void NewProcedure( ProcType P )
        {
            switch( P )
            {
                case ProcType.URLLIST:
                    ProcList.Add( new ProcUrlList() );
                    break;
                case ProcType.FIND:
                    ProcList.Add( new ProcFind() );
                    break;
                case ProcType.DOWNLOAD:
                    ProcList.Add( new ProcDownload() );
                    break;
            }
        }

        public void RemoveProcedure( Procedure P )
        {
            ProcList.Remove( P );
        }
    }
}
