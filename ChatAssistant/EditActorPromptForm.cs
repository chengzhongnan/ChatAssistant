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
    public partial class EditActorPromptForm : Form
    {
        public EditActorPromptForm()
        {
            InitializeComponent();
        }

        private void EditActorPromptForm_Load(object sender, EventArgs e)
        {

        }

        public void Init(string actorName, string prompt)
        {
            tbActorName.Text = actorName;
            tbPrompt.Text = prompt;
        }

        public string ActorName => tbActorName.Text;
        public string Prompt => tbPrompt.Text;
    }
}
