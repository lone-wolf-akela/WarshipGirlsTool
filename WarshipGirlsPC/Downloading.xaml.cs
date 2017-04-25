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
using static WarshipGirlsFinalTool.Warshipgirls;
using WarshipGirlsFinalTool;

namespace WarshipGirlsPC
{
    /// <summary>
    /// Downloading.xaml 的交互逻辑
    /// </summary>
    public partial class Downloading : Window
    {
        public Downloading()
        {
            InitializeComponent();
        }
        
        public void Update(ResDownloadStage stage, string filename, long current, long max)
        {
            switch(stage)
            {
                case ResDownloadStage.Checking:
                    lbEvent.Content = "正在校验文件：";
                    progressBarText.Text = $"{current}/{max}";
                    break;
                case ResDownloadStage.Downloading:
                    lbEvent.Content = "正在下载文件：";                  
                    progressBarText.Text = $"{current.StrFormatByteSize()}/{max.StrFormatByteSize()}";
                    break;
            }
            lbFilename.Content = System.IO.Path.GetFileName(filename);
            progress.Maximum = max;
            progress.Value = current;
        }
    }
}
