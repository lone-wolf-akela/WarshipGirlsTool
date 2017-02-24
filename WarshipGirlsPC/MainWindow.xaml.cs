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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using WarshipGirlsFinalTool;
using System.Windows.Media.Animation;
using System.Configuration;

namespace WarshipGirlsPC
{
    public class UserInfoConfig : ConfigurationSection
    {
        [ConfigurationProperty("Username", DefaultValue = "", IsRequired = true)]
        public string Username
        {
            get { return this["Username"].ToString(); }
            set { this["Username"] = value; }
        }

        [ConfigurationProperty("Password", DefaultValue = "",  IsRequired = true)]
        public string Password
        {
            get { return this["Password"].ToString(); }
            set { this["Password"] = value; }
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class SharedRes
        {
            public Warshipgirls core;
            public MainWindow winmain;
            public Window1 win1;
            public WindowGo winGo;
            public Downloading winDownload;

            public void closeAllWin()
            {
                winGo?.Close();
                winGo = null;
                winDownload?.Close();
                winDownload = null;
            }
        }
        /***************************************/
        public SharedRes res = new SharedRes();

        private int selectedServerIndex;
        /***************************************/
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            res.core?.Close();
#if (DEBUG)
#else
            try
#endif
            {
                res.core = new Warshipgirls
                {
                    version = @"2.9.0",
                    device = @"Lone Wolf PC Client/0.1.0 (Windows 10) https://github.com/lone-wolf-akela/WarshipGirlsTool",
                    username = Username.Text,
                    password = Password.Password,
                };
                if ((bool)iOS.IsChecked)
                {
                    res.core.firstSever = @"http://version.jr.moefantasy.com/";
                    res.core.market = 3;
                    res.core.channel = 1;
                }
                else if ((bool)Android.IsChecked)
                {
                    res.core.firstSever = @"http://version.jr.moefantasy.com/";
                    res.core.market = 2;
                    res.core.channel = 0;
                }
                else if ((bool)Japan.IsChecked)
                {
                    res.core.firstSever = @"http://version.jp.warshipgirls.com/";
                    res.core.market = 2;
                    res.core.channel = 0;
                }
                else
                {
                    throw new Exception("请选择一个服务器！");
                }
                res.core.Language = Warshipgirls.LANG.SChinese;
                res.core.checkVer();
                if (res.core.checkDownload())
                {
                    res.winDownload = new Downloading();
                    res.winDownload.Show();

                    res.core.downloadRes();

                    res.winDownload.Close();

                    Activate();
                }

                res.core.getInitConfigs();
                res.core.passportLogin();

                var configini = new IniFile("config.ini");
                configini.Write("USERNAME", Username.Text, "Account");
                configini.Write("PASSWORD", Password.Password, "Account");
                configini.Write("SERVER", (bool)iOS.IsChecked ? "iOS" : (bool)Android.IsChecked ? "Android" : "Japan", "Account");

                for (int i = 0; i < res.core.passportLogin_txt["serverList"].Count(); i++)
                {
                    ((Button)FindName($"btnServer{i}")).Visibility = Visibility.Visible;
                    ((Button)FindName($"btnServer{i}")).Content = (string)res.core.passportLogin_txt["serverList"][i]["name"];
                }
                for (int i = res.core.passportLogin_txt["serverList"].Count(); i < 13; i++)
                {
                    ((Button)FindName($"btnServer{i}")).Visibility = Visibility.Hidden;
                }

                for (selectedServerIndex = 0;
                    (string)res.core.passportLogin_txt["serverList"][selectedServerIndex]["id"] !=
                    (string)res.core.passportLogin_txt["defaultServer"]; selectedServerIndex++
                    )
                    ;

                btnServer.Content = (string)res.core.passportLogin_txt["serverList"][selectedServerIndex]["name"];

                Storyboard sb = FindResource("loginBoardShrink") as Storyboard;
                sb.Begin();
            }
#if (DEBUG)
#else
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }  
#endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(@"documents\hot\ccbResources");

            var configini = new IniFile("config.ini");
            Username.Text = configini.Read("USERNAME", "Account");
            Password.Password = configini.Read("PASSWORD", "Account");
            string server= configini.Read("SERVER", "Account");
            switch (server)
            {
                case "iOS": iOS.IsChecked = true; break;
                case "Android": Android.IsChecked = true; break;
                case "Japan": Japan.IsChecked = true; break;
            }

            Storyboard sb = FindResource("init") as Storyboard;
            sb.Begin();

            res.winmain = this;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
#if (DEBUG)
#else
            try
#endif
            {
                res.core.login(selectedServerIndex);
                res.core.initGame();

                res.win1 = new Window1
                {
                    res = res
                };
                res.win1.Show();

                this.Hide();
            }
#if (DEBUG)
#else
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
#endif
        }

        private void btnSelectServer_Click(object sender, RoutedEventArgs e)
        {
            selectedServerIndex = int.Parse((string)((Button)sender).Tag);
            btnServer.Content = (string)res.core.passportLogin_txt["serverList"][selectedServerIndex]["name"];

            Storyboard sb = FindResource("hideServerList") as Storyboard;
            sb.Begin();
        }

        private void btnServer_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("showServerList") as Storyboard;
            sb.Begin();
        }
    }
}
