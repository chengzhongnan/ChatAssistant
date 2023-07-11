using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

namespace ChatAssistant
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitChatGPT();
            InitPrompt();
        }

        private IChatCompletion chatGPT = null;
        private TreeNode _CurrentNode = null;
        private TreeNode lastClickNode = null;

        private string _CategoryFile = "Category.xml";
        private string _CategoryBakFile = "Category_bak.xml";
        private string _DialoguaPath = "./dialogue/";

        private void InitChatGPT()
        {
            if (Global.GPTVersion == GPT_Version.GPT_AZure_3_5)
            {
                chatGPT = new AzureChatCompletion(
                "**********",
                "***********************",
                "***********************");
            }

            if (Global.GPTVersion == GPT_Version.GPT_OpenAI_4)
            {
                chatGPT = new OpenAIChatCompletion("********",
                    "********************************************");
            }
        }

        private TreeNode? ClearPromptTreeNode()
        {
            if (promptTreeView.Nodes.Count > 0)
            {
                var rootNode = promptTreeView.Nodes[0];
                rootNode.Nodes.Clear();
                return rootNode;
            }
            else
            {
                return null;
            }
        }

        class TreeViewTag
        {
            public string Prompt { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        class TreeViewGroupTag
        {
            public string Name { get; set; } = string.Empty;
        }

        class TreeViewItemTag
        {
            public string Prompt { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public ChatHistory Chat_History { get; set; } = new ChatHistory();
        }

        private void InitPrompt()
        {
            ReloadPrompt();
        }

        private void ReloadPrompt()
        {
            var rootNode = ClearPromptTreeNode();
            if (rootNode == null)
            {
                return;
            }

            try
            {
                XElement xRoot = XElement.Load(_CategoryFile);
                foreach(var xEle in xRoot.Elements())
                {
                    // ����
                    var categoryName = xEle.Attribute("name").Value;
                    TreeNode treeNodeGroup = new TreeNode(categoryName);
                    TreeViewGroupTag groupTag = new TreeViewGroupTag() { Name = categoryName };
                    treeNodeGroup.Tag = groupTag;
                    rootNode.Nodes.Add(treeNodeGroup);

                    foreach(var xRole in xEle.Elements())
                    {
                        // ��ɫ
                        var roleName = xRole.Attribute("name").Value;
                        var rolePrompt = xRole.Value;
                        TreeNode treeNodeRole = new TreeNode(roleName);
                        TreeViewTag roleTag = new TreeViewTag() { Name = roleName, Prompt = rolePrompt };
                        treeNodeRole.Tag = roleTag;

                        treeNodeGroup.Nodes.Add(treeNodeRole);
                    }
                }
            }
            catch (Exception) { }

            rootNode.Expand();
        }

        private void promptTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowTreeNode(e.Node);
        }

        private void ShowTreeNode(TreeNode node)
        {
            var selectNode = node;
            if (selectNode == null || selectNode.Tag == null)
            {
                _CurrentNode = null;
                return;
            }

            var treeViewTag = selectNode.Tag as TreeViewTag;
            if (treeViewTag != null)
            {
                _CurrentNode = selectNode;
                ShowTreeViewTag(treeViewTag);
            }
            else if (selectNode.Tag is TreeViewItemTag)
            {
                TreeViewItemTag itemTag = selectNode.Tag as TreeViewItemTag;
                _CurrentNode = selectNode;
                ShowTreeViewTag(itemTag);
            }
            else if (selectNode.Tag is TreeViewGroupTag)
            {

            }
        }

        /// <summary>
        /// ���ұߵ�TextBox����ʾTag������
        /// </summary>
        /// <param name="tag"></param>
        private void ShowTreeViewTag(TreeViewTag tag)
        {
            StringBuilder sb = new StringBuilder();
            // ����
            sb.AppendLine($"[{tag.Prompt}]");
            sb.AppendLine("\n");
            sb.AppendLine($"���ã�����һ��\"{tag.Name}\", ��������ʲô�����ҵ���");
            sb.AppendLine();

            tb_ShowMessage.Text = sb.ToString();

            tb_ShowMessage.SelectionStart = tb_ShowMessage.Text.Length;
            tb_ShowMessage.SelectionLength = 0;
            tb_ShowMessage.ScrollToCaret();
        }

        /// <summary>
        /// ���ұߵ�TextBox����ʾTag������
        /// </summary>
        /// <param name="tag"></param>
        private void ShowTreeViewTag(TreeViewItemTag tag)
        {
            StringBuilder sb = new StringBuilder();
            // ����
            sb.AppendLine($"[{tag.Prompt}]");
            sb.AppendLine("\n");
            sb.AppendLine($"���ã�����һ��\"{tag.Name}\", ��������ʲô�����ҵ���");
            sb.AppendLine();

            // ����ʷ�е�Message����
            foreach (var message in tag.Chat_History)
            {
                if (message.Role == AuthorRole.System)
                {
                    continue;
                }

                if (message.Role == AuthorRole.User)
                {
                    sb.Append("Q : ");
                    sb.AppendLine(message.Content);
                    sb.AppendLine();
                }

                if (message.Role == AuthorRole.Assistant)
                {
                    sb.Append("A : ");
                    sb.AppendLine(message.Content);
                    sb.AppendLine();
                }
            }

            tb_ShowMessage.Text = sb.ToString();

            tb_ShowMessage.SelectionStart = tb_ShowMessage.Text.Length;
            tb_ShowMessage.SelectionLength = 0;
            tb_ShowMessage.ScrollToCaret();
        }

        private async Task ChatMessage(TreeViewItemTag tag)
        {
            tag.Chat_History.AddUserMessage(tb_UserMessage.Text);

            ShowTreeViewTag(tag);

            var reply = await chatGPT.GenerateMessageAsync(tag.Chat_History);
            tag.Chat_History.AddAssistantMessage(reply);

            ShowTreeViewTag(tag);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendChat();
        }

        private async Task SendChat()
        {
            if (_CurrentNode == null)
            {
                MessageBox.Show("����ѡ��һ��AI��ɫ������AI����");
                return;
            }

            if (string.IsNullOrEmpty(tb_UserMessage.Text))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewTag;
            if (tag != null)
            {
                // �½�һ����Item
                TreeNode node = new TreeNode("�Ի�1");
                TreeViewItemTag itemTag = new TreeViewItemTag()
                {
                    Name = tag.Name,
                    Prompt = tag.Prompt
                };
                itemTag.Chat_History = chatGPT.CreateNewChat(tag.Name);
                itemTag.Chat_History.AddSystemMessage(tag.Prompt);

                node.Tag = itemTag;
                _CurrentNode.Nodes.Add(node);
                _CurrentNode = node;

                promptTreeView.SelectedNode = node;

                await ChatMessage(itemTag);
            }
            else
            {
                var itemTag = _CurrentNode.Tag as TreeViewItemTag;
                if (itemTag != null)
                {
                    await ChatMessage(itemTag);
                }
            }
        }

        private void NewDialogueMenuItem_Click(object sender, EventArgs e)
        {
            TreeViewTag dialogueTag = null;
            TreeNode actorNode = null;

            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewTag;
            if (tag != null)
            {
                dialogueTag = tag;
                actorNode = _CurrentNode;
            }

            var oldItemTag = _CurrentNode.Tag as TreeViewItemTag;
            if (oldItemTag != null)
            {
                var parentNode = _CurrentNode.Parent as TreeNode;
                dialogueTag = parentNode.Tag as TreeViewTag;
                actorNode = parentNode;
            }

            if (dialogueTag == null)
            {
                return;
            }

            var dialogueCount = actorNode.Nodes.Count;

            // �½�һ����Item
            TreeNode node = new TreeNode($"�Ի�{dialogueCount + 1}");
            TreeViewItemTag itemTag = new TreeViewItemTag()
            {
                Name = dialogueTag.Name,
                Prompt = dialogueTag.Prompt
            };
            itemTag.Chat_History = chatGPT.CreateNewChat(dialogueTag.Name);
            itemTag.Chat_History.AddSystemMessage(dialogueTag.Prompt);

            node.Tag = itemTag;
            actorNode.Nodes.Add(node);
            _CurrentNode = node;

            promptTreeView.SelectedNode = node;
        }

        private async void tb_UserMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await SendChat();
            }
        }

        private void DeleteDialogueMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("�Ƿ�ȷ��ɾ���Ի������ģ�", "��ʾ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

            }
        }

        /// <summary>
        /// �����˵�������ʾ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // ȡ�õ�ǰAI��ɫ����ѡ����
            if (lastClickNode == null)
            {
                // ����ʾ�Ҽ��˵�
                e.Cancel = true;
                return;
            }
            var tag = lastClickNode.Tag;
            if (tag == null)
            {
                // ���ڵ㣬ֻ��ʾ����
                NewDialogueMenuItem.Visible = false;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = false;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = true;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = false;

            }
            else if (tag is TreeViewGroupTag)
            {
                // ����ڵ�
                NewDialogueMenuItem.Visible = false;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = true;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = true;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = true;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = true;
            }
            else if (tag is TreeViewTag)
            {
                // ��ɫ�ڵ�
                NewDialogueMenuItem.Visible = true;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = true;
                DeleteActorMenuItem.Visible = true;
                NewGroupMenuItem.Visible = false;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = true;
                RenameGroupMenuItem.Visible = false;
            }
            else if (tag is TreeViewItemTag)
            {
                // �Ի��ڵ�
                NewDialogueMenuItem.Visible = true;
                DeleteDialogueMenuItem.Visible = true;
                NewActorMenuItem.Visible = false;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = false;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = false;
            }
        }

        private void promptTreeView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void promptTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            // �������Ч������Ҽ�����������������Ҽ�����ڵ�Ϊѡ��״̬
            if (lastClickNode != null)
            {
                promptTreeView.SelectedNode = lastClickNode;
                lastClickNode.Checked = true;
                _CurrentNode = lastClickNode;

                ShowTreeNode(_CurrentNode);
            }
        }

        private void promptTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            var hitNode = promptTreeView.HitTest(e.X, e.Y);
            if (hitNode != null && hitNode.Node != null)
            {
                // ��Ч�ĵ���¼�
                _CurrentNode = hitNode.Node;
                lastClickNode = hitNode.Node;
            }
            else
            {
                lastClickNode = null;
            }
        }

        private void promptTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (lastClickNode != null)
                {
                    var isLeftPressCtrl = System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftCtrl) & System.Windows.Input.KeyStates.Down;
                    var isRightPressCtrl = System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.RightCtrl) & System.Windows.Input.KeyStates.Down;

                    if (isLeftPressCtrl != System.Windows.Input.KeyStates.None || isRightPressCtrl != System.Windows.Input.KeyStates.None)
                    {
                        DoDragDrop(lastClickNode, DragDropEffects.Copy);
                    }
                    else
                    {
                        DoDragDrop(lastClickNode, DragDropEffects.Move);
                    }
                }
            }
        }

        private void promptTreeView_DragDrop(object sender, DragEventArgs e)
        {
            // �ƶ��ڵ�
            var pt = ((TreeView)(sender)).PointToClient(new Point(e.X, e.Y));
            var targetNode = promptTreeView.GetNodeAt(pt);
            
            if (targetNode == null)
            {
                return;
            }

            var dragNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (dragNode == null)
            {
                return;
            }

            // ����Ƿ������ק
            if (!CheckDragDropValid(dragNode, targetNode))
            {
                return;
            }

            if (e.Effect == DragDropEffects.Move)
            {
                if (dragNode.Parent == targetNode)
                {
                    // �ƶ��ڵ㵽Topλ��
                    targetNode.Nodes.Remove(dragNode);
                    targetNode.Nodes.Insert(0, dragNode);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    // �ƶ����ڵ������λ��
                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);
                }
                else
                {
                    //��һ�������ƶ�������һ���������� ��Ҫ��֤�����������ڵ�
                    var findNode = FindSubNodeByName(targetNode.Parent, dragNode.Text);
                    if (findNode == null)
                    {
                        // ��ԭ���ĸ��ڵ���ɾ�����ƶ��Ľڵ�
                        dragNode.Parent.Nodes.Remove(dragNode);

                        // Ŀ��ڵ����Ϊ�ӽڵ�
                        targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);
                    }
                }

                // ����ѡ�нڵ�
                SetCurrentSelectNode(dragNode);
            }
            else if (e.Effect == DragDropEffects.Copy)
            {
                // ���ƽڵ㣬ֻ�н�������ĳ����ɫ���Ƶ����������вŻḴ��
                // ����������ͬ���ƶ�����

                if (dragNode.Parent == targetNode)
                {
                    // �ƶ��ڵ㵽Topλ��
                    targetNode.Nodes.Remove(dragNode);
                    targetNode.Nodes.Insert(0, dragNode);

                    // ����ѡ�нڵ�
                    SetCurrentSelectNode(dragNode);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    // �ƶ����ڵ������λ��

                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);

                    // ����ѡ�нڵ�
                    SetCurrentSelectNode(dragNode);
                }
                else
                {
                    // ֻ���ڲ�ͬ�ķ���֮��Ľ�ɫ�ſ��Ը��ƣ����Ҳ��ܴ�����ͬ�ڵ�
                    var findNode = FindSubNodeByName(targetNode.Parent, dragNode.Text);
                    if (findNode == null)
                    {
                        var tagDrag = dragNode.Tag as TreeViewTag;
                        var tagDrop = targetNode.Tag as TreeViewTag;
                        if (tagDrag != null && tagDrop != null)
                        {
                            TreeNode newNode = new TreeNode(tagDrag.Name);
                            newNode.Tag = tagDrag;

                            targetNode.Parent.Nodes.Insert(targetNode.Index + 1, newNode);
                            // ����ѡ�нڵ�
                            SetCurrentSelectNode(newNode);
                        }
                    }
                }
            }

            // ��������ļ�
            SaveCatelogyFile();
        }

        private TreeNode FindSubNodeByName(TreeNode node, string name)
        {
            if (node == null)
            {
                return null;
            }

            foreach(TreeNode child in node.Nodes)
            {
                if (child.Text == name)
                {
                    return child;
                }
            }

            return null;
        }

        private void SetCurrentSelectNode(TreeNode node)
        {
            _CurrentNode = node;
            promptTreeView.SelectedNode = _CurrentNode;
            promptTreeView.SelectedNode.Checked = true;
        }

        private bool CheckDragDropValid(TreeNode dragNode, TreeNode dropNode)
        {
            if (dragNode == null || dropNode == null)
            {
                return false;
            }

            // ��ѯ��û�н����ڵ��ƶ����ӽڵ�����
            var parentNode = dropNode.Parent;
            while(parentNode != null)
            {
                if (parentNode == dragNode)
                {
                    return false;
                }
                parentNode = parentNode.Parent;
            }

            // û������Tag�Ľڵ㲻�����ƶ���ֻ�и��ڵ�û������
            if (dragNode.Tag == null)
            {
                return false;
            }

            if (dragNode.Tag is TreeViewGroupTag)
            {
                // �ƶ��Ľڵ��Ƿ���ڵ㣬���Է��ڸ��ڵ㣬������������ڵ�����
                if (dropNode.Tag == null)
                {
                    return true;
                }
                else if (dropNode.Tag is TreeViewGroupTag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (dragNode.Tag is TreeViewTag)
            {
                // �ƶ��Ľڵ��ǽ�ɫ�ڵ㣬���Է��ڸ��ڵ㣬������������ڵ����棬����������ɫ�ڵ����棨�ƶ�TreeView��λ�ã�
                if (dropNode.Tag == null || dropNode.Tag is TreeViewGroupTag || dropNode.Tag is TreeViewTag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (dragNode.Tag is TreeViewItemTag)
            {
                // �ƶ��Ľڵ��ǶԻ��ڵ㣬���ڲ������ƶ�
                return false;
            }

            return false;
        }

        private void promptTreeView_DragEnter(object sender, DragEventArgs e)
        {
            var dragData = e.Data.GetData(typeof(TreeNode));
            if (dragData != null || dragData is TreeNode)
            {
                if (lastClickNode != null)
                {
                    var isLeftPressCtrl = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl);
                    var isRightPressCtrl = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);

                    if (isLeftPressCtrl || isRightPressCtrl)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void promptTreeView_DragLeave(object sender, EventArgs e)
        {
        }

        private void NewGroupMenuItem_Click(object sender, EventArgs e)
        {
            var currentNode = promptTreeView.TopNode;

            TreeNode node = new TreeNode("�·���");
            TreeViewGroupTag tag = new TreeViewGroupTag()
            {
                Name = node.Text
            };

            node.Tag = tag;
            currentNode.Nodes.Add(node);
        }

        private void SaveDialogueContext(TreeViewGroupTag groupTag, TreeViewTag actorTag, TreeNode actorNode)
        {

        }

        /// <summary>
        /// ����༭����XML�ļ�
        /// </summary>
        private void SaveCatelogyFile()
        {
            if (promptTreeView.Nodes.Count != 1)
            {
                return;
            }

            XElement xRoot = new XElement("categories");
            foreach(TreeNode node in promptTreeView.Nodes[0].Nodes)
            {
                if (node.Tag == null) continue;

                // ��һ���㼶���Ƿ���
                var tag = node.Tag as TreeViewGroupTag;
                if (tag == null) continue;

                var xGroup = new XElement("category", new XAttribute("name", tag.Name));
                
                
                foreach(TreeNode child in node.Nodes)
                {
                    // �ڶ����㼶���ǽ�ɫ
                    if (child.Tag == null) continue;

                    var tagActor = child.Tag as TreeViewTag;
                    if (tagActor == null) continue;

                    var xActor = new XElement("role", new XAttribute("name", tagActor.Name));
                    xActor.Value = tagActor.Prompt;
                    
                    xGroup.Add(xActor);

                    // ������ǶԻ��ڵ�
                    if (child.Nodes.Count > 0)
                    {
                        SaveDialogueContext(tag, tagActor, child);
                    }
                }
                xRoot.Add(xGroup);
            }

            System.IO.File.Copy(_CategoryFile, _CategoryBakFile, true);

            xRoot.Save(_CategoryFile);
        }

        private void NewActorMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null)
            {
                return;
            }

            TreeNode groupNode = null;

            if (_CurrentNode.Tag is TreeViewGroupTag)
            {
                groupNode = _CurrentNode;
            }

            if (_CurrentNode.Tag is TreeViewTag)
            {
                groupNode = _CurrentNode.Parent;
            }

            if (groupNode == null)
            {
                return;
            }

            EditActorPromptForm editActorPromptForm = new EditActorPromptForm();
            if (editActorPromptForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(editActorPromptForm.ActorName))
            {
                TreeNode treeNode = new TreeNode(editActorPromptForm.ActorName);
                TreeViewTag treeViewTag = new TreeViewTag() { Name = editActorPromptForm.ActorName, Prompt = editActorPromptForm.Prompt };
                treeNode.Tag = treeViewTag;

                groupNode.Nodes.Add(treeNode);

                SaveCatelogyFile();
            }
        }

        private void DeleteActorMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            if (!(_CurrentNode.Tag is TreeViewTag))
            {
                return;
            }

            if (MessageBox.Show("�Ƿ�ȷʵҪɾ���ý�ɫ", "��ʾ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _CurrentNode.Parent.Nodes.Remove(_CurrentNode);
                _CurrentNode = null;

                SaveCatelogyFile();
            }
        }

        private void EditPromptMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            if (!(_CurrentNode.Tag is TreeViewTag))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewTag;

            EditActorPromptForm editActorPromptForm = new EditActorPromptForm();
            editActorPromptForm.Init(tag.Name, tag.Prompt);

            if (editActorPromptForm.ShowDialog() == DialogResult.OK)
            {
                _CurrentNode.Text = editActorPromptForm.ActorName;
                tag.Name = editActorPromptForm.ActorName;
                tag.Prompt = editActorPromptForm.Prompt;

                SaveCatelogyFile();
            }
        }

        private void DeleteGroupMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            if (!(_CurrentNode.Tag is TreeViewGroupTag))
            {
                return;
            }

            if (MessageBox.Show("�Ƿ�ȷ��ɾ����ǰ���飿","��ʾ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                promptTreeView.Nodes.Remove(_CurrentNode);
                _CurrentNode = null;
                SaveCatelogyFile();
            }
        }

        private void RenameGroupMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            if (!(_CurrentNode.Tag is TreeViewGroupTag))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewGroupTag;

            RenameGroupForm form = new RenameGroupForm();
            form.GroupName = tag.Name;

            if (form.ShowDialog() == DialogResult.OK)
            {
                tag.Name = form.GroupName;
                _CurrentNode.Text = form.GroupName;

                SaveCatelogyFile();
            }
        }

        private void ѡ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm configForm = new ConfigForm();
            configForm.ShowDialog();

            InitChatGPT();
        }
    }
}