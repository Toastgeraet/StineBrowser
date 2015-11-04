using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using HtmlAgilityPack;

namespace StineBrowser
{
    class StineLinkTreeNode : TreeNode
    {
        public string URL { get; private set; }
        public HtmlNode HTML_NODE { get; private set; }

        public StineLinkTreeNode(HtmlNode node)
        {
            base.Text = HttpUtility.HtmlDecode(node.InnerHtml);
            URL = node.GetAttributeValue("href", "not_found");
            URL = HttpUtility.HtmlDecode(URL);            
            HTML_NODE = node;
        }

    }
}
