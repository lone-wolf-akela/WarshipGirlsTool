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

namespace WarshipGirlsPC
{
    /// <summary>
    /// WindowSelectFleet.xaml 的交互逻辑
    /// </summary>
    public partial class WindowSelectFleet : Window
    {
        /***************************************/
        public MainWindow.SharedRes res;
        public bool[] fleetAvailable = new bool[4];

        private int RetVal = 0;
        /***************************************/
        public WindowSelectFleet()
        {
            InitializeComponent();
        }

        private void btnFleet_Click(object sender, RoutedEventArgs e)
        {
            RetVal = int.Parse((string)((Button)sender).Tag);
            RefreshUI();
        }

        private void RefreshUI()
        {
            txtInfo.Text = "请选择出征舰队。";
            btnOK.IsEnabled = false;

            if(RetVal!=0)
            {
                btnOK.IsEnabled = fleetAvailable[RetVal - 1];
                txtInfo.Text += Environment.NewLine + $"当前已选择：第{RetVal}舰队。";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            res.core.music.play("move.mp3", false);
            RefreshUI();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Owner.Tag = RetVal;

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
