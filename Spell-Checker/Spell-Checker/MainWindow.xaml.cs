using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace Spell_Checker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        string comment_format = "{0}/{1}자";
        DispatcherTimer timer = new DispatcherTimer();
        string last_clipboard = "";
        System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
        HotKey hotKey;

        const int limit_characters = 1000;  //  최대 1200자 이상가능..
        public MainWindow()
        {
            InitializeComponent();


            notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            notifyIcon.Visible = true;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            lbComment.Content = string.Format(comment_format, tbInput.Text.Length, limit_characters);
            last_clipboard = Clipboard.GetText(TextDataFormat.Text);


            hotKey = new HotKey(Key.G, KeyModifier.Ctrl | KeyModifier.Win, (HotKey hk) =>
            {
                if (this.ShowInTaskbar == false)
                {
                    ShowInTaskbar = true;
                    WindowState = WindowState.Normal;
                    this.Activate();
                }
                else
                {
                    this.Activate();

                }

                string str = Clipboard.GetText(TextDataFormat.Text);
                if (string.Compare(str, last_clipboard) == 0 && string.Compare(str, tbInput.Text) == 0)
                    return;

                tbInput.Text = str;

                last_clipboard = str;
            });




            notifyIcon.DoubleClick += delegate (object sender, EventArgs e)
            {
                if (this.ShowInTaskbar == false)
                {
                    ShowInTaskbar = true;
                    WindowState = WindowState.Normal;
                    this.Activate();
                }
                else
                {
                    this.Activate();
                }
            };

            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem();
            item.Text = "종료";
            item.Click += delegate (object s, EventArgs e) { this.Close(); };
            contextMenu.MenuItems.Add(item);
            notifyIcon.ContextMenu = contextMenu;

        }



        private void TbInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //tbInput.Text = tbInput.Text.Replace("\r\n", "\n");
            if (timer.IsEnabled)
                timer.Stop();
            timer.Start();

            string temp = tbInput.Text;
            int count = 0;
            for (int i=1; i<temp.Length; ++i)
            {
                if (temp[i - 1] == '\r' && temp[i] == '\n')
                    ++count;
            }
            if (tbInput.Text.Length - count > 1000)
            {
                tbInput.Text = tbInput.Text.Replace("\r\n", "\n").Remove(limit_characters);
                tbInput.Select(tbInput.Text.Length, 0);
                lbComment.Content = string.Format(comment_format, tbInput.Text.Length, limit_characters);
            }
            else
                lbComment.Content = string.Format(comment_format, tbInput.Text.Length- count, limit_characters);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckGrammar();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckGrammar();
        }

        void CheckGrammar()
        {
            try
            {
                string html = ParseResponse(GetRequest(tbInput.Text.Replace("\r\n", "\n")));

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                foreach (var node in doc.DocumentNode.ChildNodes)
                {
                    if (node.Name == "em")
                    {
                        string color = node.Attributes["class"].Value;
                        color = color.Remove(color.Length - 5);

                        tbOuput.AppendText(node.InnerText, GetColor(color));
                    }
                    else if (node.Name == "br")
                    {
                        tbOuput.AppendText("\r\n");
                    }
                    else if (node.Name == "#text")
                    {
                        tbOuput.AppendText(node.InnerText, Colors.Black);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            if (timer.IsEnabled)
                timer.Stop();
        }
       

        Color GetColor(string str)
        {
            switch (str)
            {
                case "violet":
                    return Colors.DarkViolet;
                case "red":
                    return Colors.Red;
                case "green":
                    return Colors.Green;
                case "blue":
                    return Colors.DeepSkyBlue;

            }

            return Colors.Black;
        }

        string GetRequest(string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("https://m.search.naver.com/p/csearch/ocontent/util/SpellerProxy?_callback=jQuery11240545863063909606_1545387016835&q=");
            string html_string = WebUtility.UrlEncode(str);
            sb.Append(html_string).Append("&where=nexearch&color_blindness=0&_=1545387016837");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sb.ToString());
            req.Method = "GET";
            req.ContentType = "text/html; charset=utf-8";

            WebResponse res = req.GetResponse();

            StreamReader sr = new StreamReader(res.GetResponseStream());

            return sr.ReadToEnd();
        }

        //  @return 맞춤법이 안 맞는 html
        string ParseResponse(string str)
        {
            TextRange txt = new TextRange(tbOuput.Document.ContentStart, tbOuput.Document.ContentEnd);
            txt.Text = "";

            int idx = str.IndexOf('(');
            str = str.Remove(0, idx + 1);
            idx = str.LastIndexOf(')');
            str = str.Remove(idx, str.Length - idx);


            Response.Temp m = Newtonsoft.Json.JsonConvert.DeserializeObject<Response.Temp>(str);

            Response.Result result = m.message.result;
            return result.html.Replace("&quot;", "\""); ;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = true;
            }
            else
            {
                ShowInTaskbar = false;
            }
        }
    }
}
