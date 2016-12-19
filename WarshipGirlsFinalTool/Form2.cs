using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WarshipGirlsPNGTool;

namespace WarshipGirlsFinalTool
{
    public partial class Form2 : Form
    {
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackgroundImage == null) base.OnPaintBackground(e);
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
                int newWidth = ClientRectangle.Width;
                int newHeight = (int)(BackgroundImage.Height *
                                       ((double)ClientRectangle.Width / BackgroundImage.Width) + .5);
                int newY = -(newHeight - ClientRectangle.Height)/2;
                e.Graphics.DrawImage(BackgroundImage,
                    new Rectangle(0, newY, newWidth, newHeight));
            }
        }
        
        public Warshipgirls conn;
        public Form1 form1;

        private readonly Label[] explore = new Label[1 + 4];
        public Form2()
        {
            InitializeComponent();
            explore[1] = explore1;
            explore[2] = explore2;
            explore[3] = explore3;
            explore[4] = explore4;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            RefreshBasicData();
            setLanguage(this);
        }

        private void RefreshBasicData()
        {
            textBox1.Text = (string) conn.gameinfo["userVo"]["username"];
            textBox2.Text = (string) conn.gameinfo["userVo"]["level"];
            textBox3.Text = (string) conn.gameinfo["userVo"]["exp"];
            textBox4.Text = (string) conn.gameinfo["userVo"]["oil"];
            textBox5.Text = (string) conn.gameinfo["userVo"]["ammo"];
            textBox6.Text = (string) conn.gameinfo["userVo"]["steel"];
            textBox7.Text = (string) conn.gameinfo["userVo"]["aluminium"];

            if (DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)
                || DateTime.Now.TimeOfDay > new TimeSpan(18, 0, 0))
            {
                this.BackgroundImage = Image.FromFile(@"documents\hot\ccbResources\main_eve_bg.png");
            }
            else
            {
                this.BackgroundImage = Image.FromFile(@"documents\hot\ccbResources\main_light_bg.png");
            }

            string secretaryID = conn.gameinfo["secretary"].ToString();
            if (secretaryID == "0")
            {
                secretaryID = (from fleet in conn.gameinfo["fleetVo"]
                               where fleet["id"].ToString() == "1"
                               select fleet["ships"][0].ToString()).First();
            }
            shipPic.Image = conn.getShipImage(secretaryID,
                Warshipgirls.ShipImageType.L, false);


            Image main_source_frame1 = conn.imageFinder.getImage("main_source_frame1.png");
            Image main_source_frame2 = conn.imageFinder.getImage("main_source_frame2.png");
            Image main_source_frame3 = conn.imageFinder.getImage("main_source_frame3.png");
            Image main_source_1 = conn.imageFinder.getImage("main_source_1.png");//弹
            Image main_source_2 = conn.imageFinder.getImage("main_source_2.png");//钢
            Image main_source_3 = conn.imageFinder.getImage("main_source_3.png");//铝
            Image main_source_4 = conn.imageFinder.getImage("main_source_4.png");//油
            Bitmap main_source_frame_bitmap = 
                new Bitmap(
                    main_source_frame1.Width + 
                    main_source_frame2.Width*2+
                    main_source_frame3.Width, 
                    Math.Max(Math.Max(main_source_frame1.Height,
                    main_source_frame2.Height), 
                    main_source_frame3.Height));

            using (Graphics g = Graphics.FromImage(main_source_frame_bitmap))
            {
                //油
                g.DrawImage(main_source_frame1, 0, 0);
                g.DrawImage(main_source_4, 0, 0);
                //弹
                g.DrawImage(main_source_frame2, main_source_frame1.Width, 0);
                g.DrawImage(main_source_1, main_source_frame1.Width, 0);
                //钢
                g.DrawImage(main_source_frame2, 
                    main_source_frame1.Width+ main_source_frame2.Width, 0);
                g.DrawImage(main_source_2,
                    main_source_frame1.Width + main_source_frame2.Width, 0);
                //铝
                g.DrawImage(main_source_frame3,
                    main_source_frame1.Width + main_source_frame2.Width*2, 0);
                g.DrawImage(main_source_3,
                    main_source_frame1.Width + main_source_frame2.Width * 2, 0);
            }

            main_source_frame.Image = main_source_frame_bitmap;
        }
        private void setLanguage(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Label)
                    ((Label) ctrl).Text = conn.getLangStr(((Label) ctrl).Text);
                else
                    setLanguage(ctrl);
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.ingame = false;
            form1.Show();
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
        }

        private enum ExploreState
        {
            idle,exploring,finshed
        }

        private readonly ExploreState[] explorestate = new ExploreState[1 + 4]
        {
            ExploreState.idle, ExploreState.idle, ExploreState.idle, ExploreState.idle, ExploreState.idle
        };
        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int fleetId = 1; fleetId <= 4; fleetId++)
            {
                var info = from expinfo in conn.gameinfo["pveExploreVo"]["levels"]
                    where (string) expinfo["fleetId"] == fleetId.ToString()
                    select expinfo;
                if (!info.Any())
                {
                    explore[fleetId].Text = "空闲";
                    explorestate[fleetId]=ExploreState.idle;
                }
                else
                {
                    int time = (int)(long.Parse((string) info.First()["endTime"])
                        - DateTime.Now.ToUTC() / 1000);
                    if (time >= 0)
                    {
                        explorestate[fleetId] = ExploreState.exploring;
                        explore[fleetId].Text = new TimeSpan(0, 0, time).ToString(@"hh\:mm\:ss");
                    }
                    else
                    {
                        if (explorestate[fleetId] != ExploreState.finshed)
                            form1.notice(2000, conn.getLangStr("HasFinishedPVEExplore"),
                                string.Format(conn.getLangStr("ExpeditionCompleted")
                                .Replace("%d", "{0}").Replace("%s", "{1}"), fleetId, "???"),
                                ToolTipIcon.Info);
                        explorestate[fleetId] = ExploreState.finshed;
                        explore[fleetId].Text = conn.getLangStr("HasFinishedPVEExplore");
                    }
                }
            }  
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)
                || DateTime.Now.TimeOfDay > new TimeSpan(18, 0, 0))
            {
                conn.music.play("port-night.mp3", false);
            }
            else
            {
                conn.music.play("port-day.mp3", false);
            }
        }

        private void Form2_Deactivate(object sender, EventArgs e)
        {
            conn.music.stop();
        }
    }
}
