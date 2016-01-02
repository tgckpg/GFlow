using System;

namespace libtaotu.Parser
{
    class HtmlParser
    {
        private int cIndex;
        private string lowSrc;
        private int nextStrPos;
        public string src;

        public HtmlParser( string srcHtml )
        {
            this.src = srcHtml;
            this.lowSrc = this.src.ToLower();
            this.cIndex = 0;
        }

        public string findnext( string tag, string head, string tail )
        {
            string str = "";
            tag = tag == null ? "" : tag.ToLower();
            head = head == null ? "" : head.ToLower();
            tail = tail == null ? "" : tail.ToLower();

            Label_0042:
            while ( tag != "" )
            {
                int index = this.lowSrc.IndexOf( tag, this.cIndex );
                if ( index < 0 )
                {
                    return "";
                }
                if ( !this.nextchar( ( index + tag.Length ) - 1 ).Equals( "=" ) )
                {
                    this.cIndex = index + tag.Length;
                }
                else
                {
                    str = this.nextstring( this.nextStrPos );
                    this.cIndex = this.nextStrPos;
                    string str2 = str.ToLower();
                    if ( ( ( head == "" ) || str2.StartsWith( head ) ) && ( !( tail != "" ) || str2.EndsWith( tail ) ) )
                    {
                        return str;
                    }
                }
            }
            if ( head != "" )
            {
                int num2 = this.lowSrc.IndexOf( head, this.cIndex );
                if ( num2 < 0 )
                {
                    return "";
                }
                str = this.nextstring( num2 - 1 );
                this.cIndex = num2 + 2;
                string str3 = str.ToLower();
                if ( !( tail != "" ) || str3.EndsWith( tail ) )
                {
                    return str;
                }
                goto Label_0042;
            }
            if ( tail != "" )
            {
                int num3 = this.lowSrc.IndexOf( tail, this.cIndex );
                if ( num3 < 0 )
                {
                    return "";
                }
                str = this.getstringbytail( num3 + tail.Length );
                this.cIndex = ( num3 + tail.Length ) + 1;
            }
            return str;
        }

        public string getstringbytail( int tPos )
        {
            int startIndex = tPos - 1;
            int num2 = tPos;
            while ( ( ( ( this.lowSrc[ startIndex ] != ' ' ) && ( this.lowSrc[ startIndex ] != '"' ) ) && ( ( this.lowSrc[ startIndex ] != '=' ) && ( this.lowSrc[ startIndex ] != '\r' ) ) ) && ( ( this.lowSrc[ startIndex ] != '\n' ) && ( this.lowSrc[ startIndex ] != '\'' ) ) )
            {
                startIndex--;
                if ( startIndex == 0 )
                {
                    break;
                }
            }
            startIndex++;
            return this.src.Substring( startIndex, num2 - startIndex );
        }

        public void init()
        {
            this.cIndex = 0;
        }

        public string nextchar( int cPos )
        {
            cPos++;
            while ( ( ( this.lowSrc[ cPos ] == ' ' ) || ( this.lowSrc[ cPos ] == '\r' ) ) || ( this.lowSrc[ cPos ] == '\n' ) )
            {
                cPos++;
            }
            this.nextStrPos = cPos;
            char ch = this.lowSrc[ cPos ];
            return ch.ToString();
        }

        public string nextstring( int cPos )
        {
            string str = this.nextchar( cPos );
            cPos = this.nextStrPos;
            if ( str.Equals( "\"" ) || str.Equals( "'" ) )
            {
                this.nextchar( cPos );
            }
            int nextStrPos = this.nextStrPos;
            cPos = nextStrPos;
            while ( ( ( ( this.lowSrc[ cPos ] != ' ' ) && ( this.lowSrc[ cPos ] != '\r' ) ) && ( ( this.lowSrc[ cPos ] != '\n' ) && ( this.lowSrc[ cPos ] != '"' ) ) ) && ( ( this.lowSrc[ cPos ] != '\'' ) && ( this.lowSrc[ cPos ] != '>' ) ) )
            {
                cPos++;
                if ( cPos == this.lowSrc.Length )
                {
                    break;
                }
            }
            int num2 = cPos;
            this.nextStrPos = num2;
            return this.src.Substring( nextStrPos, num2 - nextStrPos );
        }
    }
}

