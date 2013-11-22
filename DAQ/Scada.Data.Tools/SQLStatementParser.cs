using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Tools
{
    class SQLStatementParser
    {
        StringBuilder sb = new StringBuilder();

        internal string Add(string line)
        {
            line = line.Trim();
            if (line.StartsWith("#"))
            {
                return string.Empty;
            }
            
            int p = line.IndexOf(";");
            if (p >= 0)
            {
                string part = line.Substring(0, p);
                sb.Append(part);
                string ret = sb.ToString();

                sb.Length = 0;
                sb.Append(line.Substring(p + 1));

                return ret;
            }
            else
            {
                sb.Append(line);
                return string.Empty;
            }
        }
    }
}
