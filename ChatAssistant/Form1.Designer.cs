namespace ChatAssistant
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("AI角色");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.promptTreeView = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NewActorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteActorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditPromptMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewDialogueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteDialogueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SplitGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RenameGroupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameDialogueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.tb_ShowMessage = new System.Windows.Forms.TextBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tb_UserMessage = new System.Windows.Forms.TextBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnContinue = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.角色ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选项ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.promptTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(846, 696);
            this.splitContainer1.SplitterDistance = 205;
            this.splitContainer1.TabIndex = 0;
            // 
            // promptTreeView
            // 
            this.promptTreeView.AllowDrop = true;
            this.promptTreeView.ContextMenuStrip = this.contextMenuStrip1;
            this.promptTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.promptTreeView.Location = new System.Drawing.Point(0, 0);
            this.promptTreeView.Name = "promptTreeView";
            treeNode1.Name = "Root";
            treeNode1.Text = "AI角色";
            this.promptTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.promptTreeView.Size = new System.Drawing.Size(205, 696);
            this.promptTreeView.TabIndex = 0;
            this.promptTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.promptTreeView_ItemDrag);
            this.promptTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.promptTreeView_AfterSelect);
            this.promptTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.promptTreeView_DragDrop);
            this.promptTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.promptTreeView_DragEnter);
            this.promptTreeView.DragLeave += new System.EventHandler(this.promptTreeView_DragLeave);
            this.promptTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.promptTreeView_MouseClick);
            this.promptTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.promptTreeView_MouseDown);
            this.promptTreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.promptTreeView_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewActorMenuItem,
            this.DeleteActorMenuItem,
            this.EditPromptMenuItem,
            this.NewDialogueMenuItem,
            this.DeleteDialogueMenuItem,
            this.NewGroupMenuItem,
            this.SplitGroupMenuItem,
            this.DeleteGroupMenuItem,
            this.RenameGroupMenuItem,
            this.renameDialogueMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(140, 224);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // NewActorMenuItem
            // 
            this.NewActorMenuItem.Name = "NewActorMenuItem";
            this.NewActorMenuItem.Size = new System.Drawing.Size(139, 22);
            this.NewActorMenuItem.Text = "新建角色";
            this.NewActorMenuItem.Click += new System.EventHandler(this.NewActorMenuItem_Click);
            // 
            // DeleteActorMenuItem
            // 
            this.DeleteActorMenuItem.Name = "DeleteActorMenuItem";
            this.DeleteActorMenuItem.Size = new System.Drawing.Size(139, 22);
            this.DeleteActorMenuItem.Text = "删除角色";
            this.DeleteActorMenuItem.Click += new System.EventHandler(this.DeleteActorMenuItem_Click);
            // 
            // EditPromptMenuItem
            // 
            this.EditPromptMenuItem.Name = "EditPromptMenuItem";
            this.EditPromptMenuItem.Size = new System.Drawing.Size(139, 22);
            this.EditPromptMenuItem.Text = "编辑提示词";
            this.EditPromptMenuItem.Click += new System.EventHandler(this.EditPromptMenuItem_Click);
            // 
            // NewDialogueMenuItem
            // 
            this.NewDialogueMenuItem.Name = "NewDialogueMenuItem";
            this.NewDialogueMenuItem.Size = new System.Drawing.Size(139, 22);
            this.NewDialogueMenuItem.Text = "新建对话";
            this.NewDialogueMenuItem.Click += new System.EventHandler(this.NewDialogueMenuItem_Click);
            // 
            // DeleteDialogueMenuItem
            // 
            this.DeleteDialogueMenuItem.Name = "DeleteDialogueMenuItem";
            this.DeleteDialogueMenuItem.Size = new System.Drawing.Size(139, 22);
            this.DeleteDialogueMenuItem.Text = "删除对话";
            this.DeleteDialogueMenuItem.Click += new System.EventHandler(this.DeleteDialogueMenuItem_Click);
            // 
            // NewGroupMenuItem
            // 
            this.NewGroupMenuItem.Name = "NewGroupMenuItem";
            this.NewGroupMenuItem.Size = new System.Drawing.Size(139, 22);
            this.NewGroupMenuItem.Text = "新建分组";
            this.NewGroupMenuItem.Click += new System.EventHandler(this.NewGroupMenuItem_Click);
            // 
            // SplitGroupMenuItem
            // 
            this.SplitGroupMenuItem.Name = "SplitGroupMenuItem";
            this.SplitGroupMenuItem.Size = new System.Drawing.Size(139, 22);
            this.SplitGroupMenuItem.Text = "拆分分组";
            // 
            // DeleteGroupMenuItem
            // 
            this.DeleteGroupMenuItem.Name = "DeleteGroupMenuItem";
            this.DeleteGroupMenuItem.Size = new System.Drawing.Size(139, 22);
            this.DeleteGroupMenuItem.Text = "删除分组";
            this.DeleteGroupMenuItem.Click += new System.EventHandler(this.DeleteGroupMenuItem_Click);
            // 
            // RenameGroupMenuItem
            // 
            this.RenameGroupMenuItem.Name = "RenameGroupMenuItem";
            this.RenameGroupMenuItem.Size = new System.Drawing.Size(139, 22);
            this.RenameGroupMenuItem.Text = "重命名分组";
            this.RenameGroupMenuItem.Click += new System.EventHandler(this.RenameGroupMenuItem_Click);
            // 
            // renameDialogueMenuItem
            // 
            this.renameDialogueMenuItem.Name = "renameDialogueMenuItem";
            this.renameDialogueMenuItem.Size = new System.Drawing.Size(139, 22);
            this.renameDialogueMenuItem.Text = "重命名对话";
            this.renameDialogueMenuItem.Click += new System.EventHandler(this.renameDialogueMenuItem_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.webView21);
            this.splitContainer2.Panel1.Controls.Add(this.tb_ShowMessage);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(637, 696);
            this.splitContainer2.SplitterDistance = 637;
            this.splitContainer2.TabIndex = 0;
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView21.Location = new System.Drawing.Point(0, 0);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(637, 637);
            this.webView21.TabIndex = 1;
            this.webView21.ZoomFactor = 1D;
            // 
            // tb_ShowMessage
            // 
            this.tb_ShowMessage.Location = new System.Drawing.Point(441, 362);
            this.tb_ShowMessage.Multiline = true;
            this.tb_ShowMessage.Name = "tb_ShowMessage";
            this.tb_ShowMessage.ReadOnly = true;
            this.tb_ShowMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_ShowMessage.Size = new System.Drawing.Size(124, 92);
            this.tb_ShowMessage.TabIndex = 0;
            this.tb_ShowMessage.Visible = false;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tb_UserMessage);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer3.Size = new System.Drawing.Size(637, 55);
            this.splitContainer3.SplitterDistance = 478;
            this.splitContainer3.TabIndex = 0;
            // 
            // tb_UserMessage
            // 
            this.tb_UserMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_UserMessage.Location = new System.Drawing.Point(0, 0);
            this.tb_UserMessage.Multiline = true;
            this.tb_UserMessage.Name = "tb_UserMessage";
            this.tb_UserMessage.Size = new System.Drawing.Size(478, 55);
            this.tb_UserMessage.TabIndex = 0;
            this.tb_UserMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_UserMessage_KeyDown);
            this.tb_UserMessage.AllowDrop = true;
            this.tb_UserMessage.DragEnter += new DragEventHandler(Tb_UserMessage_DragEnter);
            this.tb_UserMessage.DragDrop += new DragEventHandler(Tb_UserMessage_DragDrop);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.btnSend);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.btnContinue);
            this.splitContainer5.Size = new System.Drawing.Size(155, 55);
            this.splitContainer5.SplitterDistance = 125;
            this.splitContainer5.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSend.Location = new System.Drawing.Point(0, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(125, 55);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnContinue
            // 
            this.btnContinue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnContinue.Location = new System.Drawing.Point(0, 0);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(26, 55);
            this.btnContinue.TabIndex = 0;
            this.btnContinue.Text = "继续";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.角色ToolStripMenuItem,
            this.关于ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(9, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(98, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 角色ToolStripMenuItem
            // 
            this.角色ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选项ToolStripMenuItem});
            this.角色ToolStripMenuItem.Name = "角色ToolStripMenuItem";
            this.角色ToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.角色ToolStripMenuItem.Text = "设置";
            // 
            // 选项ToolStripMenuItem
            // 
            this.选项ToolStripMenuItem.Name = "选项ToolStripMenuItem";
            this.选项ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.选项ToolStripMenuItem.Text = "选项";
            this.选项ToolStripMenuItem.Click += new System.EventHandler(this.选项ToolStripMenuItem_Click);
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            this.关于ToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.关于ToolStripMenuItem.Text = "关于";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer4.Size = new System.Drawing.Size(846, 725);
            this.splitContainer4.SplitterDistance = 25;
            this.splitContainer4.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 725);
            this.Controls.Add(this.splitContainer4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private TreeView promptTreeView;
        private SplitContainer splitContainer2;
        private TextBox tb_ShowMessage;
        private SplitContainer splitContainer3;
        private TextBox tb_UserMessage;
        private Button btnSend;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem 新建对话ToolStripMenuItem;
        private ToolStripMenuItem DeleteDialogueMenuItem;
        private ToolStripMenuItem NewActorMenuItem;
        private ToolStripMenuItem DeleteActorMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 角色ToolStripMenuItem;
        private ToolStripMenuItem 选项ToolStripMenuItem;
        private SplitContainer splitContainer4;
        private ToolStripMenuItem 关于ToolStripMenuItem;
        private ToolStripMenuItem NewGroupMenuItem;
        private ToolStripMenuItem SplitGroupMenuItem;
        private ToolStripMenuItem DeleteGroupMenuItem;
        private ToolStripMenuItem NewDialogueMenuItem;
        private ToolStripMenuItem EditPromptMenuItem;
        private ToolStripMenuItem RenameGroupMenuItem;
        private ToolStripMenuItem renameDialogueMenuItem;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private SplitContainer splitContainer5;
        private Button btnContinue;
    }
}