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
            Init();
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
            else if (radioButton3.Checked)
            {
                return GPT_Version.GPT_OLLAMA;
            }

            return GPT_Version.None;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;

                Global.GPTVersion = GPT_Version.GPT_AZure_3_5;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;

                Global.GPTVersion = GPT_Version.GPT_OpenAI_4;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;

                Global.GPTVersion = GPT_Version.GPT_OLLAMA;
            }
        }

        public class GPTSetting
        {
            public string Model { get; set; }
            public string Key { get; set; }
        }

        public class ProxySetting
        {
            public bool UseProxy { get; set; }
            public string ProxyType { get; set; }
            public string ProxyIp { get; set; }
            public int ProxyPort { get; set; }
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(Setting.Instance.GPT.Model))
            {
                cbOpenAIModel.SelectedItem = Setting.Instance.GPT.Model;
            }

            tbOpenAIKey.Text = Setting.Instance.GPT.Key;

            SetProxyControlState(Setting.Instance.Proxy.UseProxy);

            if (Setting.Instance.Proxy.UseProxy)
            {
                cbProxyType.SelectedItem = Setting.Instance.Proxy.ProxyType;
                tbProxyIP.Text = Setting.Instance.Proxy.ProxyIp;
                tbProxyPort.Text = Setting.Instance.Proxy.ProxyPort.ToString();
            }
        }

        private void SetProxyControlState(bool useProxy)
        {
            if (useProxy)
            {
                cbUseProxy.Checked = true;
                cbProxyType.Enabled = true;
                tbProxyIP.Enabled = true;
                tbProxyPort.Enabled = true;
            }
            else
            {
                cbUseProxy.Checked = false;
                cbProxyType.Enabled = false;
                tbProxyIP.Enabled = false;
                tbProxyPort.Enabled = false;
            }
        }

        public GPTSetting GetGPTSetting()
        {
            GPTSetting setting = new GPTSetting();
            setting.Model = cbOpenAIModel.Text;
            setting.Key = tbOpenAIKey.Text;

            return setting;
        }

        public ProxySetting GetProxySetting()
        {
            ProxySetting setting = new ProxySetting();
            setting.UseProxy = cbUseProxy.Checked;
            if (Setting.Instance.Proxy.UseProxy)
            {
                setting.ProxyIp = tbProxyIP.Text;
                setting.ProxyPort = int.Parse(tbProxyPort.Text);
                setting.ProxyType = cbProxyType.Text;
            }

            return setting;
        }

        private void tbProxyPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        public string OLLamaUrl
        {
            get
            {
                return tb_LLAMA_URL.Text;
            }
        }
    }
}
