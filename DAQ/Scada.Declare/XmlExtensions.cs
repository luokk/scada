using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Scada.Declare
{
    public static class XmlExtensions
    {
        public static string Value(this XmlDocument doc, string xpath, XmlNamespaceManager nsmgr)
        {
            var node = doc.SelectSingleNode(xpath, nsmgr);
            if (node == null)
                return string.Empty;
            return node.InnerText;
        }

        public static string Value(this XmlNode node, string xpath, XmlNamespaceManager nsmgr)
        {
            var n = node.SelectSingleNode(xpath, nsmgr);
            if (n == null)
            {
                return string.Empty;
            }
            return n.InnerText;
        }
    }
}
