using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.DataModel;

namespace libtaotu.Models.Procedure
{
    internal enum ProcType
    {
        URLLIST = 1,
        FIND = 2,
        DOWNLOAD = 4,
        EXTRACT = 8,
        PAUSE = 16,
    }

    abstract class Procedure : ActiveData
    {
        public ProcType Type { get; protected set; }
        public ProcConvoy Convoy { get; protected set; }

        abstract public Task Edit();

        virtual public Task<ProcConvoy> Run( ProcConvoy Convoy )
        {
            this.Convoy = Convoy;
            return null;
        }
    }
}

