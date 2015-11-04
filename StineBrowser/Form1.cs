using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.IO;

namespace StineBrowser
{
    public partial class Form1 : Form
    {
        string baseURL = "https://www.stine.uni-hamburg.de";
        string currentPage;
        CookieAwareWebClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            doLoginAndInitialize();
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            textBox3.Text = ((StineLinkTreeNode)e.Node).URL;
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            currentPage = client.DownloadString(baseURL + textBox3.Text);
            //richTextBox1.Text = currentPage;
            LoadPage(e.Node.Text);
        }

        private void LoadPage(string pageName)
        {
            switch (pageName)
            {
                case "Status meiner Anmeldungen":
                    parseStatusAnmeldungen();
                    linkLabel1.Text = "Status meiner Anmeldungen";
                    break;

                case "Files":
                    parseFiles();
                    break;

                default:
                    break;
            }
        }

        private void parseFiles()
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(currentPage);
            listBox1.Items.Clear();
            HtmlNodeCollection lvl1_nodes, lvl2_nodes;
            lvl1_nodes = document.DocumentNode.SelectNodes("//h1");
            linkLabel1.Text += " > " + lvl1_nodes[0].InnerHtml.Replace('\r', ' ').Replace('\n', ' ');
            lvl1_nodes = document.DocumentNode.SelectNodes("//td");
            for (int i = 0; i < lvl1_nodes.Count; i++)
            {
                if (lvl1_nodes[i].InnerHtml.TrimStart(new char[] { '\r', '\n', '\t' }).StartsWith("Datei:"))
                {
                    lvl2_nodes = document.DocumentNode.SelectNodes(lvl1_nodes[i].XPath + "//a");
                    listBox1.Items.Add(new MyKeyValuePair(lvl2_nodes[0].InnerHtml, HttpUtility.HtmlDecode(lvl2_nodes[0].GetAttributeValue("href", "nolink"))));
                }
            }
        }

        private void parseStatusAnmeldungen()
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(currentPage);

            string classToFind = "eventTitle";

            HtmlNodeCollection lvl1_nodes;
            lvl1_nodes = SelectLinksByClass(document.DocumentNode, classToFind, "");
            for (int i = 0; i < lvl1_nodes.Count; i++)
            {
                listBox1.Items.Add(new MyKeyValuePair(lvl1_nodes[i].InnerHtml, HttpUtility.HtmlDecode(lvl1_nodes[i].GetAttributeValue("href", "nolink"))));

            }
        }

        private void doLoginAndInitialize()
        {
            string welcome_page = login(textBox1.Text, textBox2.Text);
            ParseNavigation(welcome_page);
            treeView1.ExpandAll();
        }

        private string login(string username, string password)
        {
            var cookieJar = new CookieContainer();
            client = new CookieAwareWebClient(cookieJar);
            client.Encoding = Encoding.UTF8;
            string response;
            response = client.DownloadString("https://www.stine.uni-hamburg.de/scripts/mgrqispi.dll?APPNAME=CampusNet&PRGNAME=STARTPAGE_DISPATCH&ARGUMENTS=-N000000000000001");
            response = client.DownloadString("https://www.stine.uni-hamburg.de/scripts/mgrqispi.dll?APPNAME=CampusNet&PRGNAME=EXTERNALPAGES&ARGUMENTS=-N000000000000001,-N000265,-Astartseite");
            string postData = "usrname={0}&pass={1}&APPNAME=CampusNet&PRGNAME=LOGINCHECK&ARGUMENTS=clino%2Cusrname%2Cpass%2Cmenuno%2Cmenu_type%2Cbrowser%2Cplatform&clino=000000000000001&menuno=000265&menu_type=classic&browser=&platform=";
            postData = string.Format(postData, HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password));
            client.Method = "POST";
            response = client.UploadString("https://www.stine.uni-hamburg.de/scripts/mgrqispi.dll", postData);
            string refresh_url = client.ResponseHeaders["REFRESH"].Split(new char[] { ';' }, 2)[1].Split(new char[] { '=' }, 2)[1];
            System.Diagnostics.Debug.Print(refresh_url);
            response = client.DownloadString("https://www.stine.uni-hamburg.de" + refresh_url);
            refresh_url = client.ResponseHeaders["REFRESH"].Split(new char[] { ';' }, 2)[1].Split(new char[] { '=' }, 2)[1];
            System.Diagnostics.Debug.Print(refresh_url);
            response = client.DownloadString("https://www.stine.uni-hamburg.de" + refresh_url);
            richTextBox1.Text = response;
            return response;
        }

        private void ParseNavigation(string response)
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(response);

            string classToFind = "depth_1";
            string class2ToFind = "depth_2";
            string class3ToFind = "depth_3";

            HtmlNodeCollection lvl1_nodes;
            lvl1_nodes = SelectLinksByClass(document.DocumentNode, classToFind, "");
            for (int i = 0; i < lvl1_nodes.Count; i++)
            {
                treeView1.Nodes.Add(new StineLinkTreeNode(lvl1_nodes[i]));
                if (lvl1_nodes[i].NextSibling != null)
                {
                    HtmlAgilityPack.HtmlNodeCollection lvl2_nodes =
                        SelectLinksByClass(document.DocumentNode, class2ToFind, lvl1_nodes[i].XPath + "/..");
                    for (int j = 0; j < lvl2_nodes.Count; j++)
                    {
                        treeView1.Nodes[i].Nodes.Add(new StineLinkTreeNode(lvl2_nodes[j]));
                        if (lvl2_nodes[j].NextSibling != null)
                        {
                            HtmlAgilityPack.HtmlNodeCollection lvl3_nodes =
                                SelectLinksByClass(document.DocumentNode, class3ToFind, lvl2_nodes[j].XPath + "/..");
                            for (int k = 0; k < lvl3_nodes.Count; k++)
                            {
                                treeView1.Nodes[i].Nodes[j].Nodes.Add(new StineLinkTreeNode(lvl3_nodes[k]));
                            }
                        }
                    }
                }
            }
        }

        private static HtmlAgilityPack.HtmlNodeCollection SelectLinksByClass(HtmlNode root, string classToFind, string parentNode)
        {
            return root.SelectNodes(string.Format(parentNode + "//a[contains(@class,'{0}')]", classToFind));
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            currentPage = client.DownloadString(baseURL + ((MyKeyValuePair)listBox1.SelectedItem).value);
            LoadPage("Files");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 1)
            {
                saveFileDialog1.FileName = listBox1.SelectedItem.ToString();
                saveFileDialog1.AddExtension = true;
                string ext = VirtualPathUtility.GetExtension(saveFileDialog1.FileName);
                string filter = ext.TrimStart('.').ToUpper() + " Dateien (*" + ext + ")|*" + ext;
                saveFileDialog1.Filter = filter;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    DownloadListBoxItem(listBox1.SelectedItem, saveFileDialog1.FileName);
                }
            }
            else if (listBox1.SelectedItems.Count > 1)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    string path = folderBrowserDialog1.SelectedPath;
                    foreach (object item in listBox1.SelectedItems)
                    {
                        DownloadListBoxItem(item, path + Path.DirectorySeparatorChar + item.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Keine (gültige) Datei zum Download ausgewählt.");
            }

        }

        private void DownloadListBoxItem(object listboxitem, string path)
        {

            client.DownloadFile(baseURL + ((MyKeyValuePair)listboxitem).value, path);

        }


    }
}
