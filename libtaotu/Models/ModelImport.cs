using Net.Astropenguin.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libtaotu.Models
{
    public class ModelImport
    {
        public static readonly string ID = typeof( ModelImport ).Name;

        private List<Procedure> Procedures;

        public ModelImport()
        {
            Procedures = new List<Procedure>();
        }

        private void Parse( string text )
        {
            try
            {
                string str2 = "\r\n";
                string[] strArray = null;
                string str3 = "";
                char[] separator = str2.ToCharArray();
                strArray = text.Split( separator );
                int index = -1;
                foreach ( string str5 in strArray )
                {
                    if ( !str5.Equals( "" ) )
                    {
                        if ( str5.StartsWith( "[" ) )
                        {
                            index++;
                        }
                        else if ( str5.StartsWith( "COMMENT=" ) )
                        {
                            str3 = str5.Substring( 8 ).Trim();
                        }
                        else
                        {
                            string str4;
                            if ( str5.StartsWith( "TYPE=" ) )
                            {
                                if ( index < 0 )
                                {
                                    index = 0;
                                }

                                str4 = str5.Substring( 5 ).Trim();
                                Procedure item = new Procedure();
                                switch ( str4 )
                                {
                                    case "EMPTYLIST":
                                        Procedures[ index ].ItemType = ProcType.EMPTYLIST;
                                        break;

                                    case "URLLIST":
                                        Procedures[ index ].ItemType = ProcType.URLLIST;
                                        break;

                                    case "FIND":
                                        Procedures[ index ].ItemType = ProcType.FIND;
                                        break;

                                    case "DOWN":
                                        Procedures[ index ].ItemType = ProcType.DOWN;
                                        break;

                                    case "NEXT":
                                        Procedures[ index ].ItemType = ProcType.NEXT;
                                        break;

                                    case "STOP":
                                        Procedures[ index ].ItemType = ProcType.STOP;
                                        break;

                                    default:
                                        Procedures[ index ].ItemType = ProcType.NULL;
                                        break;
                                }
                            }
                            else if ( str5.StartsWith( "RULE_HEAD=" ) )
                            {
                                if ( index < 0 )
                                {
                                    index = 0;
                                }
                                str4 = str5.Substring( 10 ).Trim();
                                Procedures[ index ].RuleHead = str4;
                            }
                            else if ( str5.StartsWith( "RULE_TAIL=" ) )
                            {
                                if ( index < 0 )
                                {
                                    index = 0;
                                }
                                str4 = str5.Substring( 10 ).Trim();
                                Procedures[ index ].RuleTail = str4;
                            }
                            else if ( str5.StartsWith( "RULE_TAG=" ) )
                            {
                                if ( index < 0 )
                                {
                                    index = 0;
                                }
                                str4 = str5.Substring( 9 ).Trim();
                                Procedures[ index ].RuleTag = str4;
                            }
                            else if ( str5.StartsWith( "URI=" ) )
                            {
                                if ( index < 0 )
                                {
                                    index = 0;
                                }
                                str4 = str5.Substring( 4 ).Trim();
                                Procedures[ index ].AddToUrlList( str4 );
                            }
                        }
                    }
                }
                if ( str3 != "" )
                {
                    Logger.Log( ID, str3, LogType.ERROR );
                }
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, ex.Message, LogType.ERROR );
            }
        }
    }
}
