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
    public partial class Form5 : Form
    {
        public Button[] btnFleet = new Button[4];
        public bool[] fleetAvailable = new bool[4];
        public int RetVal;

        public Form5()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
                btnFleet[i] = (Button)Controls["btnFleet" + (i + 1)];
            
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            UILoad();
            RefreshText();
        }

        private void UILoad()
        {
            for(int i=0;i<4;i++)
            {
                var i1 = i;
                btnFleet[i].Click += (sender, e) => { btnFleet_Click(i1); };
            }
            btnGO.Enabled = fleetAvailable[RetVal - 1];
        }

        private void RefreshText()
        {
            textBox1.Text = "请选择出征舰队。" + Environment.NewLine + $"当前已选择：第{RetVal}舰队。";
        }

        private void btnFleet_Click(int fleetID)
        {
            if(fleetAvailable[fleetID])
            {
                btnGO.Enabled = true;
                RetVal = fleetID + 1;
                RefreshText();
            }
            else
            {
                btnGO.Enabled = false;
            }
        }
    }
}
