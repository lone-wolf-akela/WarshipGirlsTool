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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;
using WarshipGirlsFinalTool;
using System.Windows.Forms;
using WarshipGirlsPC.Properties;
using MessageBox = System.Windows.MessageBox;
using System.IO;
using Application = System.Windows.Application;

namespace WarshipGirlsPC
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private enum ExploreState
        {
            idle, exploring, finshed
        }
        /***************************************/
        public MainWindow.SharedRes res;
        public NotifyIcon noti = new NotifyIcon();

        private DispatcherTimer timer = new DispatcherTimer();
        private string bgImg;
        private string secImg;
        private readonly ExploreState[] explorestate = new ExploreState[1 + 4]
        {
            ExploreState.idle, ExploreState.idle, ExploreState.idle, ExploreState.idle, ExploreState.idle
        };
        /***************************************/
        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += (s, ev) =>
            {
                RefreshUI();
            };
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            using(Stream icon= Application.GetResourceStream(new Uri("pack://application:,,,/WarshipGirls.ico")).Stream)
            {
                noti.Icon = new Icon(icon);
            }
            noti.MouseDoubleClick += (s, ev) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };

            noti.Text = "战舰少女R PC Client";
            noti.Visible = true;

            RefreshUI();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            res.closeAllWin();

            timer.Stop();
            
            noti.Dispose();

            Storyboard sb = res.winmain.FindResource("init") as Storyboard;
            sb.Begin();
            res.winmain.Show();
            res.winmain.Activate();
        }

        private void RefreshUI()
        {
            string newBgImg;
            if (DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)
                || DateTime.Now.TimeOfDay > new TimeSpan(18, 0, 0))
            {
                newBgImg = @"documents\hot\ccbResources\main_eve_bg";                
            }
            else
            {
                newBgImg = @"documents\hot\ccbResources\main_light_bg";                
            }
            if(newBgImg!=bgImg)
            {
                bgImg = newBgImg;
                var bgBrush = new ImageBrush
                {
                    Stretch = Stretch.UniformToFill,
                    ImageSource = res.core.imageFinder.getImage(bgImg).toImageSource()
                };
                this.Background = bgBrush;
            }
            /**************/
            string newSecImg = res.core.getShipImgName(res.core.getSecretaryID(), Warshipgirls.ShipImageType.L);
            if (secImg != newSecImg)
            {
                secImg = newSecImg;
                var secBrush = new ImageBrush
                {
                    Stretch = Stretch.UniformToFill,
                    ImageSource = res.core.imageFinder.getImage(secImg).toImageSource()
                };
                imgSecretary.Background = secBrush;
            }
            /**************/
            lb_oil.Content = (string)res.core.gameinfo["userVo"]["oil"];
            lb_ammo.Content = (string)res.core.gameinfo["userVo"]["ammo"];
            lb_steel.Content = (string)res.core.gameinfo["userVo"]["steel"];
            lb_alum.Content = (string)res.core.gameinfo["userVo"]["aluminium"];
            lb_diam.Content = (string)res.core.gameinfo["userVo"]["gold"];

            lb_lv.Content = (string)res.core.gameinfo["userVo"]["level"];
            double progress = (double)res.core.gameinfo["userVo"]["exp"] / ((double)res.core.gameinfo["userVo"]["nextExp"] + (double)res.core.gameinfo["userVo"]["exp"]);
            vb_lvProgress.Margin = new Thickness(4, 0, 287 - (287 - 22.16) * progress, 0);
            /**************/
            for (int fleetId = 1; fleetId <= 4; fleetId++)
            {
                var info = from expinfo in res.core.gameinfo["pveExploreVo"]["levels"]
                           where (string)expinfo["fleetId"] == fleetId.ToString()
                           select expinfo;
                if (!info.Any())
                {
                    explorestate[fleetId] = ExploreState.idle;
                }
                else
                {
                    string exploreTitle = (from expinfo in res.core.init_txt["pveExplore"]
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
                            noti.ShowBalloonTip(2000, res.core.getLangStr("HasFinishedPVEExplore"),
                                string.Format(res.core.getLangStr("ExpeditionCompleted")
                                .Replace("%d", "{0}").Replace("%s", "{1}"), fleetId, exploreTitle),
                                ToolTipIcon.Info);
                        explorestate[fleetId] = ExploreState.finshed;
                    }
                }
            }
            /**************/
            if (
                explorestate[0] == ExploreState.finshed ||
                explorestate[1] == ExploreState.finshed ||
                explorestate[2] == ExploreState.finshed ||
                explorestate[3] == ExploreState.finshed
                )
                ((Grid)FindName("lb_expFinished")).Visibility = Visibility.Visible;
            else
                ((Grid)FindName("lb_expFinished")).Visibility = Visibility.Hidden;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)
                || DateTime.Now.TimeOfDay > new TimeSpan(18, 0, 0))
            {
                res.core.music.play("port-night.mp3", false);
            }
            else
            {
                res.core.music.play("port-day.mp3", false);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("尚未实现！");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            res.core.music.stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            res.winGo?.Close();
            res.winGo = new WindowGo
            {
                res = res
            };
            res.winGo.Show();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                res.closeAllWin();
                Hide();
            }
        }
    }
}
