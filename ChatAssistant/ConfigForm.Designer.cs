namespace ChatAssistant
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPageGPT = new TabPage();
            groupBox1 = new GroupBox();
            label2 = new Label();
            label1 = new Label();
            tbOpenAIKey = new TextBox();
            cbOpenAIModel = new ComboBox();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            tabPageProxy = new TabPage();
            groupBox2 = new GroupBox();
            cbProxyType = new ComboBox();
            label4 = new Label();
            label5 = new Label();
            label3 = new Label();
            tbProxyPort = new TextBox();
            tbProxyIP = new TextBox();
            cbUseProxy = new CheckBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            tabControl1.SuspendLayout();
            tabPageGPT.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPageProxy.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageGPT);
            tabControl1.Controls.Add(tabPageProxy);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(528, 372);
            tabControl1.TabIndex = 0;
            // 
            // tabPageGPT
            // 
            tabPageGPT.Controls.Add(button2);
            tabPageGPT.Controls.Add(button1);
            tabPageGPT.Controls.Add(groupBox1);
            tabPageGPT.Location = new Point(4, 24);
            tabPageGPT.Name = "tabPageGPT";
            tabPageGPT.Padding = new Padding(3);
            tabPageGPT.Size = new Size(520, 344);
            tabPageGPT.TabIndex = 0;
            tabPageGPT.Text = "GPT设置";
            tabPageGPT.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(tbOpenAIKey);
            groupBox1.Controls.Add(cbOpenAIModel);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Location = new Point(17, 20);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(468, 254);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "当前GPT版本";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(57, 185);
            label2.Name = "label2";
            label2.Size = new Size(26, 15);
            label2.TabIndex = 3;
            label2.Text = "Key";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(57, 144);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 3;
            label1.Text = "Model";
            // 
            // tbOpenAIKey
            // 
            tbOpenAIKey.Location = new Point(104, 182);
            tbOpenAIKey.Name = "tbOpenAIKey";
            tbOpenAIKey.Size = new Size(332, 23);
            tbOpenAIKey.TabIndex = 2;
            // 
            // cbOpenAIModel
            // 
            cbOpenAIModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOpenAIModel.FormattingEnabled = true;
            cbOpenAIModel.Items.AddRange(new object[] { "gpt-4-0613", "gpt-4", "gpt-4-32k", "gpt-4-32k-0613", "gpt-3.5-turbo", "gpt-3.5-turbo-0613", "gpt-3.5-turbo-16k", "gpt-3.5-turbo-16k-0613" });
            cbOpenAIModel.Location = new Point(104, 141);
            cbOpenAIModel.Name = "cbOpenAIModel";
            cbOpenAIModel.Size = new Size(160, 23);
            cbOpenAIModel.TabIndex = 1;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Enabled = false;
            radioButton1.Location = new Point(35, 50);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(78, 19);
            radioButton1.TabIndex = 0;
            radioButton1.Text = "AZure API";
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Checked = true;
            radioButton2.Location = new Point(36, 97);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(86, 19);
            radioButton2.TabIndex = 0;
            radioButton2.TabStop = true;
            radioButton2.Text = "OpenAI API";
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            // 
            // tabPageProxy
            // 
            tabPageProxy.Controls.Add(button3);
            tabPageProxy.Controls.Add(button4);
            tabPageProxy.Controls.Add(groupBox2);
            tabPageProxy.Location = new Point(4, 24);
            tabPageProxy.Name = "tabPageProxy";
            tabPageProxy.Padding = new Padding(3);
            tabPageProxy.Size = new Size(520, 344);
            tabPageProxy.TabIndex = 1;
            tabPageProxy.Text = "代理设置";
            tabPageProxy.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cbProxyType);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(tbProxyPort);
            groupBox2.Controls.Add(tbProxyIP);
            groupBox2.Controls.Add(cbUseProxy);
            groupBox2.Location = new Point(29, 22);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(462, 228);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "代理";
            // 
            // cbProxyType
            // 
            cbProxyType.FormattingEnabled = true;
            cbProxyType.Items.AddRange(new object[] { "HTTP", "SOCKS5" });
            cbProxyType.Location = new Point(88, 73);
            cbProxyType.Name = "cbProxyType";
            cbProxyType.Size = new Size(103, 23);
            cbProxyType.TabIndex = 4;
            cbProxyType.Text = "HTTP";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(44, 168);
            label4.Name = "label4";
            label4.Size = new Size(29, 15);
            label4.TabIndex = 3;
            label4.Text = "Port";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(44, 76);
            label5.Name = "label5";
            label5.Size = new Size(31, 15);
            label5.TabIndex = 3;
            label5.Text = "Type";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(44, 118);
            label3.Name = "label3";
            label3.Size = new Size(17, 15);
            label3.TabIndex = 3;
            label3.Text = "IP";
            // 
            // tbProxyPort
            // 
            tbProxyPort.Location = new Point(88, 165);
            tbProxyPort.Name = "tbProxyPort";
            tbProxyPort.Size = new Size(100, 23);
            tbProxyPort.TabIndex = 2;
            tbProxyPort.KeyPress += tbProxyPort_KeyPress;
            // 
            // tbProxyIP
            // 
            tbProxyIP.Location = new Point(88, 115);
            tbProxyIP.Name = "tbProxyIP";
            tbProxyIP.Size = new Size(149, 23);
            tbProxyIP.TabIndex = 1;
            // 
            // cbUseProxy
            // 
            cbUseProxy.AutoSize = true;
            cbUseProxy.Location = new Point(44, 32);
            cbUseProxy.Name = "cbUseProxy";
            cbUseProxy.Size = new Size(78, 19);
            cbUseProxy.TabIndex = 0;
            cbUseProxy.Text = "启用代理";
            cbUseProxy.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(104, 301);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "确定";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Location = new Point(270, 301);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "取消";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.DialogResult = DialogResult.Cancel;
            button3.Location = new Point(274, 286);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 3;
            button3.Text = "取消";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.DialogResult = DialogResult.OK;
            button4.Location = new Point(108, 286);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 4;
            button4.Text = "确定";
            button4.UseVisualStyleBackColor = true;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(528, 372);
            Controls.Add(tabControl1);
            Name = "ConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ConfigForm";
            Load += ConfigForm_Load;
            tabControl1.ResumeLayout(false);
            tabPageGPT.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPageProxy.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPageGPT;
        private GroupBox groupBox1;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private TabPage tabPageProxy;
        private ComboBox cbOpenAIModel;
        private TextBox tbOpenAIKey;
        private Label label2;
        private Label label1;
        private GroupBox groupBox2;
        private Label label4;
        private Label label3;
        private TextBox tbProxyPort;
        private TextBox tbProxyIP;
        private CheckBox cbUseProxy;
        private ComboBox cbProxyType;
        private Label label5;
        private Button button2;
        private Button button1;
        private Button button3;
        private Button button4;
    }
}