using System;
using System.Collections;
using System.Collections.Generic;

namespace libtaotu.Models
{
    public enum ProcType
    {
        DOWN = 3,
        EMPTYLIST = 0,
        FIND = 2,
        NEXT = 4,
        NULL = -1,
        STOP = 5,
        URLLIST = 1
    }

    public class Procedure
    {
        public ProcType ItemType = ProcType.NULL;

        public string DownloadDirectory { get; set; }
        public string RuleHead { get; set; }
        public string RuleTag { get; set; }
        public string RuleTail { get; set; }

        private List<string> UrlList;

        public Procedure()
        {
            UrlList = new List<string>();
            Clear();
        }

        public void AddToUrlList( string sUrl )
        {
            if( !UrlList.Contains( sUrl ) )
            {
                UrlList.Add( sUrl );
            }
        }

        public void Clear()
        {
            this.ItemType = ProcType.NULL;
            this.UrlList.Clear();
            this.RuleHead = "";
            this.RuleTail = "";
            this.RuleTag = "";
        }

        public async void Edit()
        {

        }
    }
}

