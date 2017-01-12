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
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), ClientRectangle);
                int newWidth = ClientRectangle.Width;
                int newHeight = (int)(BackgroundImage.Height *
                                       ((double)ClientRectangle.Width / BackgroundImage.Width) + .5);
                int newY = -(newHeight - ClientRectangle.Height) / 2;
                e.Graphics.DrawImage(BackgroundImage,
                    new Rectangle(0, newY, newWidth, newHeight));
            }
        }
        private void Form2_ResizeEnd(object sender, EventArgs e)
        {
            main_source_frame.Location = new Point((this.Size.Width - main_source_frame.Size.Width) / 2, 0);
        }

        public Warshipgirls conn;
        public Form1 form1;
        public Form3 form3;

        private readonly Dictionary<string, Image> imageRes = new Dictionary<string, Image>();
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            UIload();
            RefreshBasicData();
            RefreshData();
            setLanguage(this);
            Form2_ResizeEnd(null, null);
        }

        private void UIload()
        {
            imageRes["main_source_frame1"] = conn.imageFinder.getImage("main_source_frame1.png");
            imageRes["main_source_frame2"] = conn.imageFinder.getImage("main_source_frame2.png");
            imageRes["main_source_frame3"] = conn.imageFinder.getImage("main_source_frame3.png");
            imageRes["main_source_1"] = conn.imageFinder.getImage("main_source_1.png");//弹
            imageRes["main_source_2"] = conn.imageFinder.getImage("main_source_2.png");//钢
            imageRes["main_source_3"] = conn.imageFinder.getImage("main_source_3.png");//铝
            imageRes["main_source_4"] = conn.imageFinder.getImage("main_source_4.png");//油

            imageRes["main_btn_sheep"] = conn.imageFinder.getImage("main_btn_sheep.png");
            imageRes["main_btn_sheep_g"] = conn.imageFinder.getImage("main_btn_sheep_g.png");

            btnDock.BackgroundImage = imageRes["main_btn_sheep"];
            btnDock.Region = helper.GetRegionFromImg(new Bitmap(imageRes["main_btn_sheep"]));
            btnDock.MouseDown += (sender, e) =>
            {
                ((Button)sender).BackgroundImage = imageRes["main_btn_sheep_g"];
            };
            btnDock.MouseUp += (sender, e) =>
            {
                ((Button)sender).BackgroundImage = imageRes["main_btn_sheep"];
            };

        }
        private void RefreshBasicData()
        {
            textBox1.Text = (string)conn.gameinfo["userVo"]["username"];
            textBox2.Text = (string)conn.gameinfo["userVo"]["level"];
            textBox3.Text = (string)conn.gameinfo["userVo"]["exp"];

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
        }

        private void RefreshData()
        {
            Bitmap main_source_frame_bitmap =
                new Bitmap(
                    imageRes["main_source_frame1"].Width +
                    imageRes["main_source_frame2"].Width * 2 +
                    imageRes["main_source_frame3"].Width,
                    Math.Max(Math.Max(imageRes["main_source_frame1"].Height,
                    imageRes["main_source_frame2"].Height),
                    imageRes["main_source_frame3"].Height));

            using (Graphics g = Graphics.FromImage(main_source_frame_bitmap))
            {
                //油
                g.DrawImage(imageRes["main_source_frame1"], 0, 0);
                g.DrawImage(imageRes["main_source_4"], 0, 0);
                g.DrawString((string)conn.gameinfo["userVo"]["oil"],
                    new Font(form1.DejaVuSansMono.Families[0], 10),
                    new SolidBrush(Color.White),
                    new RectangleF(imageRes["main_source_4"].Width, 0,
                        imageRes["main_source_frame1"].Width - imageRes["main_source_4"].Width,
                        imageRes["main_source_frame1"].Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                    );
                //弹
                g.DrawImage(imageRes["main_source_frame2"], imageRes["main_source_frame1"].Width, 0);
                g.DrawImage(imageRes["main_source_1"], imageRes["main_source_frame1"].Width, 0);
                g.DrawString((string)conn.gameinfo["userVo"]["ammo"],
                    new Font(form1.DejaVuSansMono.Families[0], 10),
                    new SolidBrush(Color.White),
                    new RectangleF(imageRes["main_source_frame1"].Width + imageRes["main_source_1"].Width, 0,
                        imageRes["main_source_frame2"].Width - imageRes["main_source_1"].Width,
                        imageRes["main_source_frame2"].Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                    );
                //钢
                g.DrawImage(imageRes["main_source_frame2"],
                    imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width, 0);
                g.DrawImage(imageRes["main_source_2"],
                    imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width, 0);
                g.DrawString((string)conn.gameinfo["userVo"]["steel"],
                    new Font(form1.DejaVuSansMono.Families[0], 10),
                    new SolidBrush(Color.White),
                    new RectangleF(imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width
                    + imageRes["main_source_2"].Width, 0,
                        imageRes["main_source_frame2"].Width - imageRes["main_source_2"].Width,
                        imageRes["main_source_frame2"].Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                    );
                //铝
                g.DrawImage(imageRes["main_source_frame3"],
                    imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width * 2, 0);
                g.DrawImage(imageRes["main_source_3"],
                    imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width * 2, 0);
                g.DrawString((string)conn.gameinfo["userVo"]["aluminium"],
                    new Font(form1.DejaVuSansMono.Families[0], 10),
                    new SolidBrush(Color.White),
                    new RectangleF(imageRes["main_source_frame1"].Width + imageRes["main_source_frame2"].Width * 2
                    + imageRes["main_source_3"].Width, 0,
                        imageRes["main_source_frame3"].Width - imageRes["main_source_3"].Width,
                        imageRes["main_source_frame3"].Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                    );
            }

            main_source_frame.Image = main_source_frame_bitmap;
        }
        private void setLanguage(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Label)
                    ((Label)ctrl).Text = conn.getLangStr(((Label)ctrl).Text);
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
            {
                form3?.Close();
                form3 = null;
                this.Hide();
            }
        }

        private enum ExploreState
        {
            idle, exploring, finshed
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
                           where (string)expinfo["fleetId"] == fleetId.ToString()
                           select expinfo;
                if (!info.Any())
                {
                    explorestate[fleetId] = ExploreState.idle;
                }
                else
                {
                    string exploreTitle = (from expinfo in conn.init_txt["pveExplore"]
                                       where (string)expinfo["id"] == (string)info.First()["exploreId"]
                                       select (string)expinfo["title"]).First();

                    int time = (int)(long.Parse((string)info.First()["endTime"])
                        - DateTime.Now.ToUTC() / 1000);
                    if (time >= 0)
                    {
                        explorestate[fleetId] = ExploreState.exploring;
                    }
                    else
                    {
                        if (explorestate[fleetId] != ExploreState.finshed)
                            form1.notice(2000, conn.getLangStr("HasFinishedPVEExplore"),
                                string.Format(conn.getLangStr("ExpeditionCompleted")
                                .Replace("%d", "{0}").Replace("%s", "{1}"), fleetId, exploreTitle),
                                ToolTipIcon.Info);
                        explorestate[fleetId] = ExploreState.finshed;
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

        private void btnDock_Click(object sender, EventArgs e)
        {
            if (form3 == null)
                form3 = new Form3 { conn = conn, form1 = form1, form2 = this };
            form3.Show();
            form3.Focus();
        }
    }
}
