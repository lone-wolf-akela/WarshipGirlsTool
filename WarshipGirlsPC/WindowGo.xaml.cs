using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WarshipGirlsFinalTool;

namespace WarshipGirlsPC
{
    /// <summary>
    /// WindowGo.xaml 的交互逻辑
    /// </summary>
    public partial class WindowGo : Window
    {
        private enum btnState { Sortie, Return, Complete }
        /***************************************/
        public MainWindow.SharedRes res;

        private DispatcherTimer timer = new DispatcherTimer();
        private int currentChapter = 1;
        private btnState[] btnExpStates = new btnState[4];
        private string[] btnExpCmdStr = new string[4];
        /***************************************/

        public WindowGo()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            res.core.music.play("move.mp3", false);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            res.core.music.stop();
        }

        private void btnExpCha_Click(object sender, RoutedEventArgs e)
        {
            currentChapter = int.Parse((string)((Button)sender).Tag);
            RefreshUI();
        }

        private void RefreshUI()
        {
            for (int i = 0; i < 4; i++)
            {
                var expInfo = (from exp in res.core.init_txt["pveExplore"]
                               where (string)exp["id"] == (currentChapter * 10000 + i + 1).ToString()
                               select exp).First();
                ((TextBox)FindName($"txtExp{i + 1}")).Text = (string)expInfo["title"] + Environment.NewLine;
                ((TextBox)FindName($"txtExp{i + 1}")).Text += res.core.getLangStr("PVEExploreRewardTitle");
                foreach (var award in (JObject)expInfo["award"])
                {
                    ((TextBox)FindName($"txtExp{i + 1}")).Text += res.core.getCidText(int.Parse(award.Key))
                        + "x" + (string)award.Value + "，";
                }
                ((TextBox)FindName($"txtExp{i + 1}")).Text.Remove(((TextBox)FindName($"txtExp{i + 1}")).Text.Length - 2);
                if (expInfo["pruductGoods"].Any())
                {
                    ((TextBox)FindName($"txtExp{i + 1}")).Text += "；" + res.core.getLangStr("PVEExploreItemTitle")
                        + res.core.getCidText((int)expInfo["pruductGoods"][0]);
                }
               ((TextBox)FindName($"txtExp{i + 1}")).Text += Environment.NewLine + res.core.getLangStr("PVEExploreNeedFlagLevelLabel")
                    + expInfo["needFlagShipLevel"] + "；"
                    + res.core.getLangStr("PVEExploreNeedShipNumLabel")
                    + expInfo["needShipNum"] + "；" +
                    res.core.getLangStr("PVEExploreNeedShipTypeLabel");
                foreach (var type in expInfo["needShipType"])
                {
                    ((TextBox)FindName($"txtExp{i + 1}")).Text += res.core.getShipTypeText((string)type["type"])
                        + "x" + (string)type["num"];
                }
                ((TextBox)FindName($"txtExp{i + 1}")).Text += Environment.NewLine;

                var fleetInExp = from fleet in res.core.gameinfo["pveExploreVo"]["levels"]
                                 where (string)fleet["exploreId"] == (currentChapter * 10000 + i + 1).ToString()
                                 select fleet;
                if (fleetInExp.Any())
                {
                    int time = (int)((long)fleetInExp.First()["endTime"] - DateTime.Now.ToUTC() / 1000);
                    if (time >= 0)
                    {
                        ((TextBox)FindName($"txtExp{i + 1}")).Text += string.Format("舰队 {0} 远征中...\t",
                            (from fleet in res.core.gameinfo["fleetVo"]
                             where (int)fleet["id"] == (int)fleetInExp.First()["fleetId"]
                             select (string)fleet["title"]).First()
                            );
                        ((TextBox)FindName($"txtExp{i + 1}")).Text += res.core.getLangStr("TimeLeft")
                        + new TimeSpan(0, 0, time).toHMS();

                        btnExpStates[i] = btnState.Return;
                        ((Button)FindName($"btnExp{i + 1}")).Content = "回港";
                    }
                    else
                    {
                        ((TextBox)FindName($"txtExp{i + 1}")).Text += string.Format("舰队 {0} 远征完成！\t",
                           (from fleet in res.core.gameinfo["fleetVo"]
                            where (int)fleet["id"] == (int)fleetInExp.First()["fleetId"]
                            select (string)fleet["title"]).First()
                           );

                        btnExpStates[i] = btnState.Complete;
                        ((Button)FindName($"btnExp{i + 1}")).Content = "完成";
                    }
                    btnExpCmdStr[i] = (currentChapter * 10000 + i + 1).ToString();
                }
                else
                {
                    ((TextBox)FindName($"txtExp{i + 1}")).Text += res.core.getLangStr("TimeLeft")
                        + new TimeSpan(0, 0, (int)expInfo["needTime"]).toHMS();

                    btnExpStates[i] = btnState.Sortie;
                    ((Button)FindName($"btnExp{i + 1}")).Content = "出征";
                    btnExpCmdStr[i] = (currentChapter * 10000 + i + 1).ToString();
                }
            }
        }

        private void btnExp_Click(object sender, RoutedEventArgs e)
        {
            int btnID = int.Parse((string)(((Button)sender).Tag)) - 1;
            switch (btnExpStates[btnID])
            {
                case btnState.Sortie:
                    var winSelectFleet = new WindowSelectFleet
                    {
                        res = res
                    };
                    winSelectFleet.Owner = this;

                    int fleetMaxNum = (int)res.core.gameinfo["userVo"]["fleetMaxNum"];
                    for (int i = 0; i < fleetMaxNum; i++)
                    {
                        var fleetInfo = (from fleet in res.core.gameinfo["fleetVo"]
                                         where (int)fleet["id"] == i + 1
                                         select fleet).First();

                        var expInfo = (from exp in res.core.init_txt["pveExplore"]
                                       where (string)exp["id"] == btnExpCmdStr[btnID]
                                       select exp).First();

                        winSelectFleet.fleetAvailable[i] = true;
                        //舰队是否空闲？
                        if ((int)fleetInfo["status"] != 0)
                            winSelectFleet.fleetAvailable[i] = false;
                        //船只数量是否足够？
                        else if (fleetInfo["ships"].Count() < (int)expInfo["needShipNum"])
                            winSelectFleet.fleetAvailable[i] = false;
                        //旗舰等级是否足够？
                        else if (
                            (from shipinfo in res.core.gameinfo["userShipVO"]
                             join fleetdata in res.core.gameinfo["fleetVo"] on (int)shipinfo["id"] equals fleetdata["ships"].Any() ? (int)fleetdata["ships"][0] : -1
                             where (int)fleetdata["id"] == i + 1
                             where (int)shipinfo["level"] < (int)expInfo["needFlagShipLevel"]
                             select shipinfo
                             ).Any()
                            )
                            winSelectFleet.fleetAvailable[i] = false;
                        //船只类型是否满足？
                        else
                        {
                            foreach (var type in expInfo["needShipType"])
                            {
                                if ((from shipinfo in res.core.gameinfo["userShipVO"]
                                     join shipdata in res.core.init_txt["shipCard"] on (int)shipinfo["shipCid"] equals (int)shipdata["cid"]
                                     where (int)shipinfo["fleetId"] == i + 1
                                     where (int)shipdata["type"] == (int)type["type"]
                                     select shipinfo
                                    ).Count() < (int)type["num"]
                                )
                                {
                                    winSelectFleet.fleetAvailable[i] = false;
                                }
                            }
                        }
                        //船只补给是否满？
                        if ((from shipinfo in res.core.gameinfo["userShipVO"]
                             where (int)shipinfo["fleetId"] == i + 1
                             where (int)shipinfo["battleProps"]["hp"] == (int)shipinfo["battlePropsMax"]["hp"]
                             where (int)shipinfo["battleProps"]["oil"] == (int)shipinfo["battlePropsMax"]["oil"]
                             where (int)shipinfo["battleProps"]["ammo"] == (int)shipinfo["battlePropsMax"]["ammo"]
                             where (int)shipinfo["battleProps"]["aluminium"] == (int)shipinfo["battlePropsMax"]["aluminium"]
                             select shipinfo
                            ).Count()
                            != fleetInfo["ships"].Count()
                        )
                        {
                            winSelectFleet.fleetAvailable[i] = false;
                        }
                    }
                    for (int i = fleetMaxNum; i < 4; i++)
                    {
                        ((Button)winSelectFleet.FindName($"btnFleet{i + 1}")).IsEnabled = false;
                    }
                    if ((bool)winSelectFleet.ShowDialog())
                    {
#if (DEBUG)
#else
                    try
#endif
                        {
                            res.core.explore_start(btnExpCmdStr[btnID], Tag.ToString());
                        }
#if (DEBUG)
#else
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
#endif
                    }

                    break;
                case btnState.Return:
#if (DEBUG)
#else
                    try
#endif
                    {
                        res.core.explore_cancel(btnExpCmdStr[btnID]);
                    }
#if (DEBUG)
#else
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
#endif
                    break;
                case btnState.Complete:
#if (DEBUG)
#else
                    try
#endif
                    {
                        res.core.explore_getResult(btnExpCmdStr[btnID]);
                    }
#if (DEBUG)
#else
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += (s, ev) =>
            {
                RefreshUI();
            };
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            RefreshUI();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
}
