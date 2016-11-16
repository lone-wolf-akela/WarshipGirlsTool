using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WarshipGirlsFinalTool
{
    public partial class Form1 : Form
    {
        private Form2 form2;
        private Warshipgirls conn;
        //private string udid;

        public bool ingame = false;
        public void notice(int timeout,string title,string text, ToolTipIcon icon)
        {
            this.notifyIcon1.ShowBalloonTip(timeout, title, text, icon);
        }
        public Form1()
        {
            InitializeComponent();
            var configini = new IniFile("config.ini");
            /*if (configini.KeyExists("UDID", "Machine"))
            {
                udid = configini.Read("UDID", "Machine");
            }
            else
            {
                udid = helper.GetNewUDID();
                configini.Write("UDID", udid, "Machine");
            }*/
            Username.Text = configini.Read("USERNAME", "Account");
            Password.Text = configini.Read("PASSWORD", "Account");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            conn = new Warshipgirls
            {
                version = @"2.7.0",
                username = Username.Text,
                password = Password.Text,
                //udid = udid
            };
            if (iOS.Checked)
            {
                conn.market = 3;
                conn.channel = 2;
            }
            else if (Android.Checked)
            {
                conn.market = 2;
                conn.channel = 0;
            }
            else
            {
                throw new Exception("Please Choose a Server!");
            }
            conn.checkVer();
            conn.login();
            listBox1.Items.Clear();
            foreach (string server in conn.serverList)
            {
                listBox1.Items.Add(server);
            }

            var configini = new IniFile("config.ini");
            configini.Write("USERNAME", Username.Text, "Account");
            configini.Write("PASSWORD", Password.Text, "Account");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            conn.enter(listBox1.SelectedIndex);
            form2 = new Form2
            {
                form1 = this,
                conn = conn
            };
            form2.Show();
            ingame = true;
            this.Hide();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (ingame)
            {
                form2.Show();
                form2.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }
    }
}

/*
 //请求
            string uri = @"http://version.jr.moefantasy.com/index/checkVer/2.7.0/4/0&market=4&channel=0&version=2.7.0";
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";                            //请求方法
            request.ProtocolVersion = new Version(1, 1);   //Http/1.1版本
            request.UserAgent = @"Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H)";
            request.Host = "version.jr.moefantasy.com";
            //request.Connection = @"Keep-Alive";


            //Add Other ...
            var response =request.GetResponse() as HttpWebResponse;
            //Header
            foreach (var item in response.Headers)
            {
                textBox1.Text += item.ToString() + ": " +
                                 response.GetResponseHeader(item.ToString()) + "\n";
            }

            //如果主体信息不为空，则接收主体信息内容
            if (response.ContentLength <= 0)
                return;
            //接收响应主体信息

            byte[] bytes;

            using (Stream stream = response.GetResponseStream())
            {
                int totalLength = (int)response.ContentLength;
                int numBytesRead = 0;
                bytes = new byte[totalLength + 1024];
                //通过一个循环读取流中的数据，读取完毕，跳出循环
                while (numBytesRead < totalLength)
                {
                    int num = stream.Read(bytes, numBytesRead, 1024);  //每次希望读取1024字节
                    if (num == 0)   //说明流中数据读取完毕
                        break;
                    numBytesRead += num;
                }


            }
            //将接收到的主体数据显示到界面
            string content = Encoding.UTF8.GetString(bytes);
            textBox1.Text += content;
*/