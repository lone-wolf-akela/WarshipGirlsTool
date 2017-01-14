using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WarshipGirlsFinalTool
{
    public partial class Form4 : Form
    {
        public Form1.SharedRes sharedRes;

        public Form formSource;
        public int defaultFleet;

        private Button[] btnExpChas = new Button[7];
        private TextBox[] txtExps = new TextBox[4];
        private Button[] btnExps = new Button[4];

        private enum btnState { Sortie, Return, Complete }

        private btnState[] btnExpStates = new btnState[4];
        private string[] btnExpCmdStr = new string[4];
        private int currentExpChapter = 1;

        public Form4()
        {
            InitializeComponent();

            for (int i = 0; i < 7; i++)
                btnExpChas[i] = (Button)tabExp.Controls["btnExpCha" + (i + 1)];
            for(int i=0;i<4;i++)
            {
                txtExps[i] = (TextBox)tabExp.Controls["txtExp" + (i + 1)];
                btnExps[i] = (Button)tabExp.Controls["btnExp" + (i + 1)];
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            UILoad();
            RefreshData();
        }

        private void UILoad()
        {
            for (int i = 0; i < 7; i++)
            {
                var i1 = i;
                btnExpChas[i].Click += (sender, e) => 
                {
                    currentExpChapter = i1 + 1;
                    RefreshData();
                };
            }

            for(int i=0;i<4;i++)
            {
                var i1 = i;
                btnExps[i].Click += (sender, e) => { btnExp_Click(i1); };
            }
        }

        private void RefreshData()
        {
            for(int i=0;i<4;i++)
            {
                var expInfo = (from exp in sharedRes.conn.init_txt["pveExplore"]
                               where (string)exp["id"] == (currentExpChapter * 10000 + i + 1).ToString()
                               select exp).First();
                txtExps[i].Text = (string)expInfo["title"] + Environment.NewLine;
                txtExps[i].Text += sharedRes.conn.getLangStr("PVEExploreRewardTitle");
                foreach (var award in (JObject)expInfo["award"])
                {
                    txtExps[i].Text += sharedRes.conn.getCidText(int.Parse(award.Key))
                        + "x" + (string)award.Value + "，";
                }
                txtExps[i].Text.Remove(txtExps[i].Text.Length - 2);
                if (expInfo["pruductGoods"].Any())
                {
                    txtExps[i].Text += "；" + sharedRes.conn.getLangStr("PVEExploreItemTitle")
                        + sharedRes.conn.getCidText((int)expInfo["pruductGoods"][0]);
                }
                txtExps[i].Text += Environment.NewLine + sharedRes.conn.getLangStr("PVEExploreNeedFlagLevelLabel")
                    + expInfo["needFlagShipLevel"] + "；"
                    + sharedRes.conn.getLangStr("PVEExploreNeedShipNumLabel")
                    + expInfo["needShipNum"] + "；" +
                    sharedRes.conn.getLangStr("PVEExploreNeedShipTypeLabel");
                foreach(var type in expInfo["needShipType"])
                {
                    txtExps[i].Text += sharedRes.conn.getShipTypeText((string)type["type"])
                        + "x" + (string)type["num"];
                }
                txtExps[i].Text += Environment.NewLine;

                var fleetInExp = from fleet in sharedRes.conn.gameinfo["pveExploreVo"]["levels"]
                                 where (string)fleet["exploreId"] == (currentExpChapter * 10000 + i + 1).ToString()
                                 select fleet;
                if(fleetInExp.Any())
                {
                    int time = (int)((long)fleetInExp.First()["endTime"] - DateTime.Now.ToUTC() / 1000);
                    if(time>=0)
                    {
                        txtExps[i].Text += string.Format("舰队 {0} 远征中...\t",
                            (from fleet in sharedRes.conn.gameinfo["fleetVo"]
                             where (int)fleet["id"] == (int)fleetInExp.First()["fleetId"]
                             select (string)fleet["title"]).First()
                            );
                        txtExps[i].Text += sharedRes.conn.getLangStr("TimeLeft")
                        + new TimeSpan(0, 0, time).toHMS();

                        btnExpStates[i] = btnState.Return;
                        btnExps[i].Text = "回港";
                    }
                    else
                    {
                        txtExps[i].Text += string.Format("舰队 {0} 远征完成！\t",
                           (from fleet in sharedRes.conn.gameinfo["fleetVo"]
                            where (int)fleet["id"] == (int)fleetInExp.First()["fleetId"]
                            select (string)fleet["title"]).First()
                           );

                        btnExpStates[i] = btnState.Complete;
                        btnExps[i].Text = "完成";
                    }
                    btnExpCmdStr[i] = (currentExpChapter * 10000 + i + 1).ToString();
                }
                else
                {
                    txtExps[i].Text += sharedRes.conn.getLangStr("TimeLeft")
                        + new TimeSpan(0, 0, (int)expInfo["needTime"]).toHMS();

                    btnExpStates[i] = btnState.Sortie;
                    btnExps[i].Text = "出征";
                    btnExpCmdStr[i] = (currentExpChapter * 10000 + i + 1).ToString();
                }

            }
        }

        private void btnExp_Click(int btnID)
        {
            switch (btnExpStates[btnID])
            {
                case btnState.Sortie:
                    using (var form5 = new Form5())
                    {
                        form5.RetVal = defaultFleet;
                        int fleetMaxNum = (int)sharedRes.conn.gameinfo["userVo"]["fleetMaxNum"];
                        for (int i = 0; i < fleetMaxNum; i++)
                        {
                            var fleetInfo = (from fleet in sharedRes.conn.gameinfo["fleetVo"]
                                             where (int)fleet["id"] == i + 1
                                             select fleet).First();

                            var expInfo= (from exp in sharedRes.conn.init_txt["pveExplore"]
                                          where (string)exp["id"] == btnExpCmdStr[btnID]
                                          select exp).First();

                            form5.fleetAvailable[i] = true;
                            //舰队是否空闲？
                            if ((int)fleetInfo["status"] != 0)
                                form5.fleetAvailable[i] = false;
                            //船只数量是否足够？
                            else if(fleetInfo["ships"].Count()< (int)expInfo["needShipNum"])
                                form5.fleetAvailable[i] = false;
                            //船只类型是否满足？
                            else
                            {                                
                                foreach (var type in expInfo["needShipType"])
                                {
                                    if((from shipinfo in sharedRes.conn.gameinfo["userShipVO"]
                                        join shipdata in sharedRes.conn.init_txt["shipCard"] on (int)shipinfo["shipCid"] equals (int)shipdata["cid"]
                                        where (int)shipinfo["fleetId"] == i + 1
                                        where (int)shipdata["type"] == (int)type["type"]
                                        select shipinfo
                                        ).Count()< (int)type["num"]
                                    )
                                    {
                                        form5.fleetAvailable[i] = false;
                                    }
                                }
                            }
                            //船只补给是否满？
                            if((from shipinfo in sharedRes.conn.gameinfo["userShipVO"]
                                where (int)shipinfo["fleetId"] == i + 1
                                where (int)shipinfo["battleProps"]["hp"] == (int)shipinfo["battlePropsMax"]["hp"]
                                where (int)shipinfo["battleProps"]["oil"]==(int)shipinfo["battlePropsMax"]["oil"]
                                where (int)shipinfo["battleProps"]["ammo"] == (int)shipinfo["battlePropsMax"]["ammo"]
                                where (int)shipinfo["battleProps"]["aluminium"] == (int)shipinfo["battlePropsMax"]["aluminium"]
                                select shipinfo
                                ).Count()
                                != fleetInfo["ships"].Count()
                            )
                            {
                                form5.fleetAvailable[i] = false;
                            }
                        }
                        for (int i = fleetMaxNum; i < 4; i++)
                        {
                            form5.btnFleet[i].Enabled = false;
                        }
                        if (form5.ShowDialog() == DialogResult.OK)
                        {
                            sharedRes.conn.explore_start(btnExpCmdStr[btnID], form5.RetVal.ToString());
                            
                        }
                    }
                    break;
                case btnState.Return:
                    try
                    {
                        sharedRes.conn.explore_cancel(btnExpCmdStr[btnID]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                    break;
                case btnState.Complete:
                    try
                    {
                        sharedRes.conn.explore_getResult(btnExpCmdStr[btnID]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Form4_Activated(object sender, EventArgs e)
        {
            sharedRes.conn.music.play("move.mp3", false);
        }

        private void Form4_Deactivate(object sender, EventArgs e)
        {
            sharedRes.conn.music.stop();
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            sharedRes.form4 = null;
            formSource.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshData();
        }
    }
}
