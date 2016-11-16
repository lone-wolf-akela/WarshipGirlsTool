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
    public partial class Form2 : Form
    {
        
        public Warshipgirls conn;
        public Form1 form1;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            textBox1.Text = conn.gameInfo.username;
            textBox2.Text = conn.gameInfo.level;
            textBox3.Text = conn.gameInfo.exp;
            textBox4.Text = conn.gameInfo.oil.ToString();
            textBox5.Text = conn.gameInfo.ammo.ToString();
            textBox6.Text = conn.gameInfo.steel.ToString();
            textBox7.Text = conn.gameInfo.aluminium.ToString();
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

        enum ExploreState
        {
            idle,exploring,finshed
        }

        private ExploreState[] explorestate = new ExploreState[4]
            {
                ExploreState.idle, ExploreState.idle, ExploreState.idle, ExploreState.idle
            };
        private void timer1_Tick(object sender, EventArgs e)
        {
            bool[] state = new bool[4] {false, false, false, false};
            for (int i = 0; i < conn.gameInfo.exploreinfo.exploreCount; i++)
            {
                Label label;
                switch (conn.gameInfo.exploreinfo.fleetId[i])
                {
                    case "1":
                        label = explore1;
                        state[0] = true;
                        break;
                    case "2":
                        label = explore2;
                        state[1] = true;
                        break;
                    case "3":
                        label = explore3;
                        state[2] = true;
                        break;
                    case "4":
                        label = explore4;
                        state[3] = true;
                        break;
                    default:
                        throw new Exception("Invalid fleetId!");
                }
                int time = (int)(conn.gameInfo.exploreinfo.endTime[i]-DateTime.Now.ToUTC()/1000);
                if (time >= 0)
                {
                    explorestate[i] = ExploreState.exploring;
                    label.Text = new TimeSpan(0, 0, time).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    if (explorestate[i] != ExploreState.finshed)
                        form1.notice(2000, "远征提醒", string.Format("第{0}舰队的远征完成了！", i + 1), ToolTipIcon.Info);
                    explorestate[i]=ExploreState.finshed;
                    label.Text = "完成！";
                }
            }
            if (!state[0]) explore1.Text = "空闲";
            if (!state[1]) explore2.Text = "空闲";
            if (!state[2]) explore3.Text = "空闲";
            if (!state[3]) explore4.Text = "空闲";
        }
    }
}
