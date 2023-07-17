﻿using System;
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
    public partial class RenameGroupForm : Form
    {
        public RenameGroupForm(bool bNew = false)
        {
            InitializeComponent();

            if (bNew)
            {
                this.Text = "新增分组";
            }
        }

        public string GroupName { get => textBox1.Text; set => textBox1.Text = value; }
    }
}
