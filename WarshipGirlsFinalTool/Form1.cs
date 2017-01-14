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
        public class SharedRes
        {
            public Form1 form1;
            public Form2 form2;
            public Form3 form3;
            public Form4 form4;

            public Warshipgirls conn;

            public bool ingame = false;
            public PrivateFontCollection DejaVuSansMono = new PrivateFontCollection();
            public PrivateFontCollection CloudYuanCuGBK = new PrivateFontCollection();
            public PrivateFontCollection msyhbd = new PrivateFontCollection();
        }

        public SharedRes sharedRes = new SharedRes();

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

            sharedRes.DejaVuSansMono.AddFontFile(@"documents\font\DejaVuSansMono.ttf");
            sharedRes.CloudYuanCuGBK.AddFontFile(@"documents\font\CloudYuanCuGBK.ttf");
            sharedRes.msyhbd.AddFontFile(@"documents\font\msyhbd.ttf");

            sharedRes.form1 = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sharedRes.conn = new Warshipgirls
                {
                    version = @"2.8.1",
                    device = @"Lone Wolf PC Client/0.0.5 (Windows 10) https://github.com/lone-wolf-akela/WarshipGirlsTool",
                    username = Username.Text,
                    password = Password.Text,
                };
                if (iOS.Checked)
                {
                    sharedRes.conn.firstSever = @"http://version.jr.moefantasy.com/";
                    sharedRes.conn.market = 3;
                    sharedRes.conn.channel = 1;
                }
                else if (Android.Checked)
                {
                    sharedRes.conn.firstSever = @"http://version.jr.moefantasy.com/";
                    sharedRes.conn.market = 2;
                    sharedRes.conn.channel = 0;
                }
                else if (Japan.Checked)
                {
                    sharedRes.conn.firstSever = @"http://version.jp.warshipgirls.com/";
                    sharedRes.conn.market = 2;
                    sharedRes.conn.channel = 0;
                }
                else
                {
                    throw new Exception("Please Choose a Server!");
                }
                sharedRes.conn.Language = Warshipgirls.LANG.SChinese;
                sharedRes.conn.checkVer();
                sharedRes.conn.getInitConfigs();
                sharedRes.conn.downloadRes(this);
                sharedRes.conn.passportLogin();
                listBox1.Items.Clear();
                foreach (var server in sharedRes.conn.passportLogin_txt["serverList"])
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
                sharedRes.conn.login(listBox1.SelectedIndex);
                sharedRes.conn.initGame();
                sharedRes.form2 = new Form2
                {
                    sharedRes = sharedRes
                };
                sharedRes.form2.Show();
                sharedRes.ingame = true;
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
            if (sharedRes.ingame)
            {
                sharedRes.form2.Show();
                sharedRes.form2.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
