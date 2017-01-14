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
        public Form1.SharedRes sharedRes;
        private readonly Dictionary<string, Image> imageRes = new Dictionary<string, Image>();

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
            imageRes["main_source_frame1"] = sharedRes.conn.imageFinder.getImage("main_source_frame1.png");
            imageRes["main_source_frame2"] = sharedRes.conn.imageFinder.getImage("main_source_frame2.png");
            imageRes["main_source_frame3"] = sharedRes.conn.imageFinder.getImage("main_source_frame3.png");
            imageRes["main_source_1"] = sharedRes.conn.imageFinder.getImage("main_source_1.png");//弹
            imageRes["main_source_2"] = sharedRes.conn.imageFinder.getImage("main_source_2.png");//钢
            imageRes["main_source_3"] = sharedRes.conn.imageFinder.getImage("main_source_3.png");//铝
            imageRes["main_source_4"] = sharedRes.conn.imageFinder.getImage("main_source_4.png");//油

            imageRes["main_btn_sheep"] = sharedRes.conn.imageFinder.getImage("main_btn_sheep.png");
            imageRes["main_btn_sheep_g"] = sharedRes.conn.imageFinder.getImage("main_btn_sheep_g.png");

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
            textBox1.Text = (string)sharedRes.conn.gameinfo["userVo"]["username"];
            textBox2.Text = (string)sharedRes.conn.gameinfo["userVo"]["level"];
            textBox3.Text = (string)sharedRes.conn.gameinfo["userVo"]["exp"];

            if (DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)
                || DateTime.Now.TimeOfDay > new TimeSpan(18, 0, 0))
            {
                this.BackgroundImage = Image.FromFile(@"documents\hot\ccbResources\main_eve_bg.png");
            }
            else
            {
                this.BackgroundImage = Image.FromFile(@"documents\hot\ccbResources\main_light_bg.png");
            }

            string secretaryID = sharedRes.conn.gameinfo["secretary"].ToString();
            if (secretaryID == "0")
            {
                secretaryID = (from fleet in sharedRes.conn.gameinfo["fleetVo"]
                               where fleet["id"].ToString() == "1"
                               select fleet["ships"][0].ToString()).First();
            }
            shipPic.Image = sharedRes.conn.getShipImage(secretaryID,
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
                g.DrawString((string)sharedRes.conn.gameinfo["userVo"]["oil"],
                    new Font(sharedRes.DejaVuSansMono.Families[0], 10),
                    new SolidBrush(Color.White),
                    new RectangleF(imageRes["main_source_4"].Width, 0,
                        imageRes["main_source_frame1"].Width - imageRes["main_source_4"].Width,
                        imageRes["main_source_frame1"].Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                    );
                //弹
                g.DrawImage(imageRes["main_source_frame2"], imageRes["main_source_frame1"].Width, 0);
                g.DrawImage(imageRes["main_source_1"], imageRes["main_source_frame1"].Width, 0);
                g.DrawString((string)sharedRes.conn.gameinfo["userVo"]["ammo"],
                    new Font(sharedRes.DejaVuSansMono.Families[0], 10),
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
                g.DrawString((string)sharedRes.conn.gameinfo["userVo"]["steel"],
                    new Font(sharedRes.DejaVuSansMono.Families[0], 10),
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
                g.DrawString((string)sharedRes.conn.gameinfo["userVo"]["aluminium"],
                    new Font(sharedRes.DejaVuSansMono.Families[0], 10),
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
                    ((Label)ctrl).Text = sharedRes.conn.getLangStr(((Label)ctrl).Text);
                else
                    setLanguage(ctrl);
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            sharedRes.ingame = false;
            sharedRes.form1.Show();
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                sharedRes.form3?.Close();
                sharedRes.form3 = null;
                sharedRes.form4?.Close();
                sharedRes.form4 = null;
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
                var info = from expinfo in sharedRes.conn.gameinfo["pveExploreVo"]["levels"]
                           where (string)expinfo["fleetId"] == fleetId.ToString()
                           select expinfo;
                if (!info.Any())
                {
                    explorestate[fleetId] = ExploreState.idle;
                }
                else
                {
                    string exploreTitle = (from expinfo in sharedRes.conn.init_txt["pveExplore"]
                                       where (string)expinfo["id"] == (string)info.First()["exploreId"]
                                       select (string)expinfo["title"]).First();

                    int time = (int)((long)info.First()["endTime"] - DateTime.Now.ToUTC() / 1000);
                    if (time >= 0)
                    {
                        explorestate[fleetId] = ExploreState.exploring;
                    }
                    else
                    {
                        if (explorestate[fleetId] != ExploreState.finshed)
                            sharedRes.form1.notice(2000, sharedRes.conn.getLangStr("HasFinishedPVEExplore"),
                                string.Format(sharedRes.conn.getLangStr("ExpeditionCompleted")
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
                sharedRes.conn.music.play("port-night.mp3", false);
            }
            else
            {
                sharedRes.conn.music.play("port-day.mp3", false);
            }
        }

        private void Form2_Deactivate(object sender, EventArgs e)
        {
            sharedRes.conn.music.stop();
        }

        private void btnDock_Click(object sender, EventArgs e)
        {
            if (sharedRes.form3 == null)
                sharedRes.form3 = new Form3 { sharedRes = sharedRes };
            sharedRes.form3.Show();
            sharedRes.form3.Focus();
        }
    }
}
