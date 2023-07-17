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
    public partial class RenameDialogueForm : Form
    {
        public RenameDialogueForm(bool bNew = false)
        {
            InitializeComponent();

            if (bNew)
            {
                this.Text = "新增对话";
            }
        }

        public string DialogueName { get => textBox1.Text; set => textBox1.Text = value; }
    }
}
