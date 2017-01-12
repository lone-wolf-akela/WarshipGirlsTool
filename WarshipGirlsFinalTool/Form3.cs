using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace WarshipGirlsFinalTool
{
    public partial class Form3 : Form
    {
        public Form1 form1;
        public Form2 form2;
        public Warshipgirls conn;

        private PictureBox[] FleetPics = new PictureBox[4];
        private Button[] btnFleepOps = new Button[4];
        private enum fleetState
        {
            idle, exploring, expfinshed, repairing, locked
        }
        private fleetState[] FleetStates = { fleetState.idle, fleetState.idle, fleetState.idle, fleetState.idle };
        public Form3()
        {
            InitializeComponent();
            FleetPics[0] = FleetPic1;
            FleetPics[1] = FleetPic2;
            FleetPics[2] = FleetPic3;
            FleetPics[3] = FleetPic4;

            btnFleepOps[0] = btnFleepOp1;
            btnFleepOps[1] = btnFleepOp2;
            btnFleepOps[2] = btnFleepOp3;
            btnFleepOps[3] = btnFleepOp4;
        }

        private void Form3_Activated(object sender, EventArgs e)
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

        private void Form3_Deactivate(object sender, EventArgs e)
        {
            conn.music.stop();
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            form2.form3 = null;
            form2.Focus();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            UILoad();
            RefreshData();
        }

        private void UILoad()
        {
            imageRes["ship_boader_ing"] = conn.imageFinder.getImage("ship_boader_ing.png");

            for (int i = 0; i < 4; i++)
            {
                var i1 = i;
                btnFleepOps[i].Click += (sender, e) => { btnFleetOp_Click(i1 + 1); };
            }
        }

        private void RefreshData()
        {
            for (int i = 0; i < 4; i++)
            {
                Bitmap tmpBitmap = new Bitmap(imageRes["ship_boader_ing"]);
                using (Graphics g = Graphics.FromImage(tmpBitmap))
                {
                    g.DrawString(String.Format("第{0}舰队", i + 1),
                        new Font(form1.msyhbd.Families[0], 10),
                        new SolidBrush(Color.Black),
                        new RectangleF(12, 12,
                            FleetPics[i].Width - 12,
                            FleetPics[i].Height - 12),
                        new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near }
                        );

                    string fleetstate;
                    var info = from expinfo in conn.gameinfo["pveExploreVo"]["levels"]
                               where (string)expinfo["fleetId"] == (i + 1).ToString()
                               select expinfo;
                    if (info.Any())
                    {
                        int time = (int)(long.Parse((string)info.First()["endTime"])
                                          - DateTime.Now.ToUTC() / 1000);
                        if (time >= 0)
                        {
                            fleetstate = new TimeSpan(0, 0, time).ToString(@"hh\:mm\:ss");
                            FleetStates[i] = fleetState.exploring;
                            btnFleepOps[i].Text = "回港";
                        }
                        else
                        {
                            fleetstate = conn.getLangStr("HasFinishedPVEExplore");
                            FleetStates[i] = fleetState.expfinshed;
                            btnFleepOps[i].Text = "完成";
                        }
                    }
                    else
                    {
                        fleetstate = "空闲";
                        FleetStates[i] = fleetState.idle;
                        btnFleepOps[i].Text = "出征";
                    }

                    g.DrawString(fleetstate,
                        new Font(form1.msyhbd.Families[0], 14),
                        new SolidBrush(Color.Black),
                        new RectangleF(0, 0,
                            FleetPics[i].Width,
                            FleetPics[i].Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
                        );
                }
                FleetPics[i].Image = tmpBitmap;
            }
        }

        private readonly Dictionary<string, Image> imageRes = new Dictionary<string, Image>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void btnFleetOp_Click(int fleetID)
        {
            switch (FleetStates[fleetID - 1])
            {
                case fleetState.idle:
                    //TODO:出征
                    break;
                case fleetState.exploring:
                    try
                    {
                        if (MessageBox.Show(conn.getLangStr("PVEBackSubtitle"),
                            conn.getLangStr("PVEBackToPort"), MessageBoxButtons.YesNo)
                            == DialogResult.Yes)
                        {
                            var exploreId = from expinfo in conn.gameinfo["pveExploreVo"]["levels"]
                                            where (string)expinfo["fleetId"] == fleetID.ToString()
                                            select (string)expinfo["exploreId"];
                            conn.explore_cancel(exploreId.First());
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                    break;
                case fleetState.expfinshed:
                    try
                    {
                        string exploreId = (from expinfo in conn.gameinfo["pveExploreVo"]["levels"]
                                            where (string)expinfo["fleetId"] == fleetID.ToString()
                                            select (string)expinfo["exploreId"]).First();
                        string exploreTitle = (from expinfo in conn.init_txt["pveExplore"]
                                               where (string)expinfo["id"] == exploreId
                                               select (string)expinfo["title"]).First();

                        JsonText Res = conn.explore_getResult(exploreId);

                        string resStr = string.Format(conn.getLangStr("ExpeditionCompleted")
                                .Replace("%d", "{0}").Replace("%s", "{1}"),
                                Res["shipVO"]["id"], exploreTitle);

                        if ((string)Res["bigSuccess"] == "1")
                            resStr += "\n大成功！";

                        resStr += "\n获得：\n";
                        foreach (var award in (JObject)Res["newAward"])
                        {
                            resStr += "\t" + conn.getCidText(int.Parse(award.Key)) + "x" + (string)award.Value + "\n";
                        }

                        resStr += "舰队成员：\n";
                        int shipIndex = 0;
                        //int intimacyOtherIndex = 0;
                        foreach (var shipID in (JArray)Res["shipVO"]["ships"])
                        {
                            int shipCid = (from ship in conn.gameinfo["userShipVO"] where ship["id"].ToString() == (string)shipID select int.Parse((string)ship["shipCid"])).First();
                            resStr += "\t" + conn.getCidText(shipCid) + "\t";
                            if (Res["loveChange"].HasValues &&
                                Res["loveChange"][shipIndex].ToString().ToLower() != "false")
                            {
                                resStr += "好感度:+" + Res["loveChange"][shipIndex] + "\n";
                                var shipinfo = from ship in conn.gameinfo["userShipVO"]
                                               where ship["id"].ToString() == (string)shipID
                                               select ship;
                                shipinfo.First()["love"] =
                                    int.Parse((string)shipinfo.First()["love"]) +
                                    int.Parse((string)Res["loveChange"][shipIndex]);
                            }
                            else
                            {
                                resStr += "\n";
                            }
                            shipIndex++;
                        }

                        MessageBox.Show(resStr, conn.getLangStr("HasFinishedPVEExplore"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                    break;
                case fleetState.repairing:
                    //TODO:快修
                    break;
                case fleetState.locked:
                    //TODO:NOTHING
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
