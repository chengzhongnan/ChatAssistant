using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAssistant
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            if (Global.GPTVersion == GPT_Version.GPT_AZure_3_5)
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
            else if (Global.GPTVersion == GPT_Version.GPT_OpenAI_4)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
            }
        }

        public GPT_Version GetGPT_Version()
        {
            if (radioButton1.Checked)
            {
                return GPT_Version.GPT_AZure_3_5;
            }
            else if (radioButton2.Checked)
            {
                return GPT_Version.GPT_OpenAI_4;
            }

            return GPT_Version.None;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;

                Global.GPTVersion = GPT_Version.GPT_AZure_3_5;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;

                Global.GPTVersion = GPT_Version.GPT_OpenAI_4;
            }
        }
    }
}
