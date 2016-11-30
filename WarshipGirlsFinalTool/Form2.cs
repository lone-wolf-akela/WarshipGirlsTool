using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WarshipGirlsPNGTool;

namespace WarshipGirlsFinalTool
{
    public partial class Form2 : Form
    {
        
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
            string secretaryID = conn.gameinfo["secretary"].ToString();
            if (secretaryID == "0")
            {
                secretaryID = (from fleet in conn.gameinfo["fleetVo"]
                    where fleet["id"].ToString() == "1"
                    select fleet["ships"][0].ToString()).First();
            }
            var secretary = (from ship in conn.gameinfo["userShipVO"]
                where ship["id"].ToString() == secretaryID
                select ship).First();
            string secretaryModel;
            if (secretary["skin_cid"].ToString() != "0")
            {
                secretaryModel = (from skin in conn.init_txt["ShipSkin"]
                    where skin["cid"].ToString() ==
                          secretary["skin_cid"].ToString()
                    select skin["skinId"].ToString()).First();
            }
            else
            {
                secretaryModel = (from ship in conn.init_txt["shipCard"]
                    where ship["cid"].ToString() ==
                          secretary["shipCid"].ToString()
                    select ship["picId"].ToString()).First();
            }
            secretaryModel = @"documents\hot\ccbResources\model\M_NORMAL_"
                             + secretaryModel + ".muka";
            if (!File.Exists(secretaryModel))
            {
                secretaryModel += "R";
            }
            shipPic.Image = WSGPNG.getShipModel(secretaryModel);
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
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.ingame = false;
            form1.Show();
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            if(this.WindowState==FormWindowState.Minimized)
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
                            form1.notice(2000, "远征提醒", string.Format("第{0}舰队的远征完成了！", fleetId), ToolTipIcon.Info);
                        explorestate[fleetId] = ExploreState.finshed;
                        explore[fleetId].Text = "完成！";
                    }
                }
            }  
        }
    }
}
