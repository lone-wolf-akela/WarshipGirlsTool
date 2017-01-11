using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using WarshipGirlsPNGTool;

namespace WarshipGirlsFinalTool
{
    public partial class Form1 : Form
    {
        private Form2 form2;
        private Warshipgirls conn;

        public bool ingame = false;
        public PrivateFontCollection DejaVuSansMono = new PrivateFontCollection();
        public PrivateFontCollection CloudYuanCuGBK = new PrivateFontCollection();
        public PrivateFontCollection msyhbd = new PrivateFontCollection();
        public void notice(int timeout,string title,string text, ToolTipIcon icon)
        {
            this.notifyIcon1.ShowBalloonTip(timeout, title, text, icon);
        }
        public Form1()
        {
            InitializeComponent();
            var configini = new IniFile("config.ini");
            Username.Text = configini.Read("USERNAME", "Account");
            Password.Text = configini.Read("PASSWORD", "Account");

            DejaVuSansMono.AddFontFile(@"documents\font\DejaVuSansMono.ttf");
            CloudYuanCuGBK.AddFontFile(@"documents\font\CloudYuanCuGBK.ttf");
            msyhbd.AddFontFile(@"documents\font\msyhbd.ttf");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                conn = new Warshipgirls
                {
                    version = @"2.8.0",
                    username = Username.Text,
                    password = Password.Text,
                };
                if (iOS.Checked)
                {
                    conn.firstSever = @"http://version.jr.moefantasy.com/";
                    conn.market = 3;
                    conn.channel = 1;
                }
                else if (Android.Checked)
                {
                    conn.firstSever = @"http://version.jr.moefantasy.com/";
                    conn.market = 2;
                    conn.channel = 0;
                }
                else if (Japan.Checked)
                {
                    conn.firstSever = @"http://version.jp.warshipgirls.com/";
                    conn.market = 2;
                    conn.channel = 0;
                }
                else
                {
                    throw new Exception("Please Choose a Server!");
                }
                conn.Language = Warshipgirls.LANG.SChinese;
                conn.checkVer();
                conn.getInitConfigs();
                conn.downloadRes(this);
                conn.passportLogin();
                listBox1.Items.Clear();
                foreach (var server in conn.passportLogin_txt["serverList"])
                {
                    listBox1.Items.Add((string) server["name"]);
                }

                var configini = new IniFile("config.ini");
                configini.Write("USERNAME", Username.Text, "Account");
                configini.Write("PASSWORD", Password.Text, "Account");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                conn.login(listBox1.SelectedIndex);
                conn.initGame();
                form2 = new Form2
                {
                    form1 = this,
                    conn = conn
                };
                form2.Show();
                ingame = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
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
