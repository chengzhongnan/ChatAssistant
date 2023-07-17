using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Data.Sqlite;
using SQLitePCL;

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
            // InitPrompt();
            InitSqliteDB();
        }

        private IChatCompletion chatGPT = null;
        private TreeNode _CurrentNode = null;
        private TreeNode lastClickNode = null;
        private SqliteConnection _connection;

        private string _CategoryFile = "Category.xml";
        private string _CategoryBakFile = "Category_bak.xml";
        private string _DialoguaFile = "dialogue.db";

        private void InitChatGPT()
        {
            if (Global.GPTVersion == GPT_Version.GPT_AZure_3_5)
            {
                chatGPT = new AzureChatCompletion(
                "*******",
                "*******",
                "*******");
            }

            if (Global.GPTVersion == GPT_Version.GPT_OpenAI_4)
            {
                chatGPT = new OpenAIChatCompletion("*******",
                    "*******");
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
            public int Id { get; set; }
        }

        class TreeViewGroupTag
        {
            public string Name { get; set; } = string.Empty;
        }

        class TreeViewItemTag
        {
            public int Id { get; set; }
            public string Prompt { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public ChatHistory Chat_History { get; set; } = new ChatHistory();
            public int ActorId { get; set; }
        }

        private void InitPrompt()
        {
            ReloadPrompt();
        }

        private void InitSqliteDB()
        {
            _connection = new SqliteConnection($"Data Source={_DialoguaFile}");
            _connection.Open();
                
            ReloadActors(_connection);
            ReloadDialogues(_connection);
        }

        private void ReloadActors(SqliteConnection conn)
        {
            var rootNode = ClearPromptTreeNode();
            if (rootNode == null)
            {
                return;
            }

            var groups = GetAllGroups(conn);

            foreach(var group in groups)
            {
                TreeNode treeNodeGroup = new TreeNode(group);
                TreeViewGroupTag groupTag = new TreeViewGroupTag() { Name = group };
                treeNodeGroup.Tag = groupTag;
                rootNode.Nodes.Add(treeNodeGroup);

                var actors = GetGroupActors(group);
                foreach(var actor in actors)
                {
                    TreeNode treeNodeRole = new TreeNode(actor.Name);
                    treeNodeRole.Tag = actor;

                    treeNodeGroup.Nodes.Add(treeNodeRole);
                }
            }

            rootNode.Expand();
        }

        private void ReloadDialogues(SqliteConnection conn)
        {
            var command = conn.CreateCommand();
            // 取得所有对话
            command.CommandText = @"select id, actorid, name from dialogue";
            List<TreeViewItemTag> itemTagList = new List<TreeViewItemTag>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    if (reader.IsDBNull(1) || reader.IsDBNull(2)) continue;

                    var actorid = reader.GetInt32(1);
                    var name = reader.GetString(2);

                    TreeViewItemTag itemTag = new TreeViewItemTag()
                    {
                        Id = id,
                        Name = name,
                        ActorId = actorid
                    };

                    itemTagList.Add(itemTag);
                }
            }

            ReloadDialogContext(itemTagList);
        }

        private void ReloadDialogContext(List<TreeViewItemTag> itemTagList)
        {
            var rootNode = promptTreeView.Nodes[0];
            foreach(TreeNode groupNode in rootNode.Nodes)
            {
                TreeViewGroupTag treeViewGroupTag = groupNode.Tag as TreeViewGroupTag;
                if (treeViewGroupTag == null) continue;

                foreach(TreeNode actorNode in groupNode.Nodes)
                {
                    TreeViewTag actorTag = actorNode.Tag as TreeViewTag;
                    if (actorTag == null) continue;

                    var dialogList = itemTagList.Where(a => a.ActorId == actorTag.Id).ToList();
                    foreach(var dialog in dialogList)
                    {
                        var chatHistory = LoadDialogMessage(dialog.Id);
                        RebuildDialogueNode(chatHistory, dialog, actorNode);
                    }
                }
            }
        }

        private ChatHistory LoadDialogMessage(int dialogueId)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"select id, role, message from chatmessage where dialogueid={dialogueId}";

            using(var reader = cmd.ExecuteReader())
            {
                ChatHistory chatHistory = new ChatHistory();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    if (reader.IsDBNull(1) || reader.IsDBNull(2)) continue;
                    var role = reader.GetString(1);
                    var message = reader.GetString(2);

                    if (Enum.TryParse(typeof(ChatHistory.AuthorRoles), role, out var roleEnum))
                    {
                        var roleid = (ChatHistory.AuthorRoles)roleEnum;
                        switch (roleid)
                        {
                            case ChatHistory.AuthorRoles.Assistant:
                                chatHistory.AddAssistantMessage(message);
                                break;
                            case ChatHistory.AuthorRoles.User:
                                chatHistory.AddUserMessage(message);
                                break;
                            case ChatHistory.AuthorRoles.System:
                                chatHistory.AddSystemMessage(message);
                                break;
                            case ChatHistory.AuthorRoles.Unknown:
                                break;
                        }
                    }
                }

                return chatHistory;
            }
        }

        private void RebuildDialogueNode(ChatHistory chatMessages, TreeViewItemTag tagDialogue, TreeNode actorNode)
        {
            // 加入系统提示词
            TreeViewTag tagActor = actorNode.Tag as TreeViewTag;
            if (tagActor == null)
            {
                return;
            }

            chatMessages.AddSystemMessage(tagActor.Prompt);
            tagDialogue.Chat_History = chatMessages;
            tagDialogue.Prompt = tagActor.Prompt;

            TreeNode dialogueNode = new TreeNode(tagDialogue.Name);
            dialogueNode.Tag = tagDialogue;

            actorNode.Nodes.Add(dialogueNode);
        }

        private void InsertChatMessage(int dialogueId, string roleId, string message)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO \"chatmessage\"(\"dialogueid\", \"role\", \"message\") VALUES(@dialogueid, @role, @message)";
            cmd.Parameters.AddWithValue("@dialogueid", dialogueId);
            cmd.Parameters.AddWithValue("@role", roleId);
            cmd.Parameters.AddWithValue("@message", message);

            cmd.ExecuteNonQuery();
        }

        private void renameDialogue(TreeViewItemTag itemTag, string newName)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update dialogue set name=@name where id=@id";
            cmd.Parameters.AddWithValue("@name", newName);
            cmd.Parameters.AddWithValue("@id", itemTag.Id);

            cmd.ExecuteNonQuery();
        }

        private List<string> GetAllGroups(SqliteConnection conn)
        {
            List<string> groups = new List<string>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT \"group\" FROM \"actor\"";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var group = reader.GetString(0);
                    groups.Add(group);
                }
            }
            return groups;
        }

        private List<TreeViewTag> GetGroupActors(string groupName)
        {
            List<TreeViewTag> results = new List<TreeViewTag>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, actor, prompt FROM \"actor\" where \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", groupName);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeViewTag tag = new TreeViewTag();
                    tag.Id = reader.GetInt32(0);
                    if (reader.IsDBNull(1))
                    {
                        continue;
                    }
                    else
                    {
                        tag.Name = reader.GetString(1);
                    }
                    if (!reader.IsDBNull(2))
                    {
                        tag.Prompt = reader.GetString(2);
                    }

                    results.Add(tag);
                }
            }

            return results;
        }

        private void GetDialogueTree(SqliteCommand cmd, int treeid)
        {
            cmd.CommandText = @"select group, actor, dialogue, prompt from dialogueTree where id = $id";
            cmd.Parameters.AddWithValue("$id", treeid);

            using(var reader = cmd.ExecuteReader())
            {
                while( reader.Read())
                {
                    var group = reader.GetString(0);
                    var actor = reader.GetString(1);
                    var dialogue = reader.GetString(2);
                    var prompt = reader.GetString(3);

                    return;
                }
            }
        }

        private int InsertNewDialogue(int actorid, string name)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO \"dialogue\"(\"actorid\", \"name\") VALUES(@actorid, @name); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@actorid", actorid);
            cmd.Parameters.AddWithValue("@name", name);

            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    var id = reader.GetInt32(0);
                    return id;
                }
            }

            return 0;
        }

        private void RemoveGroup(TreeViewGroupTag tag)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "delete from actor where \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", tag.Name);

            cmd.ExecuteNonQuery();
        }

        private void RemoveActor(int actorId)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"delete from actor where id={actorId}";

            cmd.ExecuteNonQuery();
        }

        private void UpdateActor(string groupName, TreeViewTag actorTag)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update actor set \"group\"=@group, actor=@actor, prompt=@prompt where id=@id";
            cmd.Parameters.AddWithValue("@group", groupName);
            cmd.Parameters.AddWithValue("@actor", actorTag.Name);
            cmd.Parameters.AddWithValue("@prompt", actorTag.Prompt);
            cmd.Parameters.AddWithValue("@id", actorTag.Id);

            cmd.ExecuteNonQuery();
        }

        private int InsertActor(string groupName, string actorName, string prompt)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO \"actor\"(\"group\", \"actor\", \"prompt\") VALUES(@group, @actor, @prompt); SELECT last_insert_rowid(); ";
            cmd.Parameters.AddWithValue("@prompt", prompt);
            cmd.Parameters.AddWithValue("@actor", actorName);
            cmd.Parameters.AddWithValue("@group", groupName);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);

                    return id;
                }
            }

            return 0;
        }

        private void UpdateGroupName(string oldName, string newName)
        {
            var actors = GetGroupActors(oldName);
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update actor set \"group\"=@group where \"group\"=@oldName";
            cmd.Parameters.AddWithValue("@group", newName);
            cmd.Parameters.AddWithValue("@oldName", oldName);

            cmd.ExecuteNonQuery();
        }

        private bool AddNewGroup(string groupName)
        {
            var cmd = _connection.CreateCommand();

            cmd.CommandText = "SELECT id FROM actor where \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", groupName);
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    return false;
                }
            }

            cmd.CommandText = "INSERT INTO actor(\"group\") VALUES(@group)";
            cmd.ExecuteNonQuery();

            return true;
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
                    // 分类
                    var categoryName = xEle.Attribute("name").Value;
                    TreeNode treeNodeGroup = new TreeNode(categoryName);
                    TreeViewGroupTag groupTag = new TreeViewGroupTag() { Name = categoryName };
                    treeNodeGroup.Tag = groupTag;
                    rootNode.Nodes.Add(treeNodeGroup);

                    foreach(var xRole in xEle.Elements())
                    {
                        // 角色
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
        /// 在右边的TextBox中显示Tag的内容
        /// </summary>
        /// <param name="tag"></param>
        private void ShowTreeViewTag(TreeViewTag tag)
        {
            StringBuilder sb = new StringBuilder();
            // 标题
            sb.AppendLine($"[{tag.Prompt}]");
            sb.AppendLine("\n");
            sb.AppendLine($"您好，我是一个\"{tag.Name}\", 请问您有什么想问我的吗？");
            sb.AppendLine();

            tb_ShowMessage.Text = sb.ToString();

            tb_ShowMessage.SelectionStart = tb_ShowMessage.Text.Length;
            tb_ShowMessage.SelectionLength = 0;
            tb_ShowMessage.ScrollToCaret();
        }

        /// <summary>
        /// 在右边的TextBox中显示Tag的内容
        /// </summary>
        /// <param name="tag"></param>
        private void ShowTreeViewTag(TreeViewItemTag tag)
        {
            StringBuilder sb = new StringBuilder();
            // 标题
            sb.AppendLine($"[{tag.Prompt}]");
            sb.AppendLine("\n");
            sb.AppendLine($"您好，我是一个\"{tag.Name}\", 请问您有什么想问我的吗？");
            sb.AppendLine();

            // 将历史中的Message加入
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
            // 保存对话
            InsertChatMessage(tag.Id, AuthorRole.User.Label, tb_UserMessage.Text);

            var reply = await chatGPT.GenerateMessageAsync(tag.Chat_History);
            tag.Chat_History.AddAssistantMessage(reply);

            ShowTreeViewTag(tag);
            // 保存对话
            InsertChatMessage(tag.Id, AuthorRole.Assistant.Label, reply);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendChat();
        }

        private async Task SendChat()
        {
            if (_CurrentNode == null)
            {
                MessageBox.Show("请先选中一个AI角色，再向AI提问");
                return;
            }

            if (string.IsNullOrEmpty(tb_UserMessage.Text))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewTag;
            if (tag != null)
            {
                var dialogueCount = _CurrentNode.Nodes.Count;
                var name = $"对话{dialogueCount + 1}";
                var dialogueId = InsertNewDialogue(tag.Id, name);
                if (dialogueId <= 0)
                {
                    MessageBox.Show("插入对话失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 新建一个子Item
                TreeNode node = new TreeNode(name);
                TreeViewItemTag itemTag = new TreeViewItemTag()
                {
                    Id = dialogueId,
                    Name = name,
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
            var name = $"对话{dialogueCount + 1}";
            var dialogueId = InsertNewDialogue(tag.Id, name);
            if (dialogueId <= 0)
            {
                MessageBox.Show("插入对话失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 新建一个子Item
            TreeNode node = new TreeNode(name);
            TreeViewItemTag itemTag = new TreeViewItemTag()
            {
                Id = dialogueId,
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
            if (MessageBox.Show("是否确定删除对话上下文？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

            }
        }

        /// <summary>
        /// 弹出菜单定制显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 取得当前AI角色树的选中项
            if (lastClickNode == null)
            {
                // 不显示右键菜单
                e.Cancel = true;
                return;
            }
            var tag = lastClickNode.Tag;
            if (tag == null)
            {
                // 根节点，只显示分组
                NewDialogueMenuItem.Visible = false;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = false;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = true;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = false;
                renameDialogueMenuItem.Visible = false;
            }
            else if (tag is TreeViewGroupTag)
            {
                // 分组节点
                NewDialogueMenuItem.Visible = false;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = true;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = true;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = true;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = true;
                renameDialogueMenuItem.Visible = false;
            }
            else if (tag is TreeViewTag)
            {
                // 角色节点
                NewDialogueMenuItem.Visible = true;
                DeleteDialogueMenuItem.Visible = false;
                NewActorMenuItem.Visible = true;
                DeleteActorMenuItem.Visible = true;
                NewGroupMenuItem.Visible = false;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = true;
                RenameGroupMenuItem.Visible = false;
                renameDialogueMenuItem.Visible = false;
            }
            else if (tag is TreeViewItemTag)
            {
                // 对话节点
                NewDialogueMenuItem.Visible = true;
                DeleteDialogueMenuItem.Visible = true;
                NewActorMenuItem.Visible = false;
                DeleteActorMenuItem.Visible = false;
                NewGroupMenuItem.Visible = false;
                SplitGroupMenuItem.Visible = false;
                DeleteGroupMenuItem.Visible = false;
                EditPromptMenuItem.Visible = false;
                RenameGroupMenuItem.Visible = false;
                renameDialogueMenuItem.Visible = true;
            }
        }

        private void promptTreeView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void promptTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            // 如果是有效的鼠标右键点击，在这里设置右键点击节点为选中状态
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
                // 有效的点击事件
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
            // 移动节点
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

            // 检查是否可以拖拽
            if (!CheckDragDropValid(dragNode, targetNode))
            {
                return;
            }

            if (e.Effect == DragDropEffects.Move)
            {
                if (dragNode.Parent == targetNode)
                {
                    // 移动节点到Top位置
                    targetNode.Nodes.Remove(dragNode);
                    targetNode.Nodes.Insert(0, dragNode);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    // 移动到节点下面的位置
                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);
                }
                else
                {
                    //从一个分组移动到另外一个分组下面 ，要保证不能有重名节点
                    var findNode = FindSubNodeByName(targetNode.Parent, dragNode.Text);
                    if (findNode == null)
                    {
                        // 从原来的父节点中删除被移动的节点
                        dragNode.Parent.Nodes.Remove(dragNode);

                        // 目标节点加入为子节点
                        targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);
                    }
                }

                // 设置选中节点
                SetCurrentSelectNode(dragNode);
            }
            else if (e.Effect == DragDropEffects.Copy)
            {
                // 复制节点，只有将分组中某个角色复制到其他分组中才会复制
                // 其他操作等同于移动操作

                if (dragNode.Parent == targetNode)
                {
                    // 移动节点到Top位置
                    targetNode.Nodes.Remove(dragNode);
                    targetNode.Nodes.Insert(0, dragNode);

                    // 设置选中节点
                    SetCurrentSelectNode(dragNode);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    // 移动到节点下面的位置

                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(targetNode.Index + 1, dragNode);

                    // 设置选中节点
                    SetCurrentSelectNode(dragNode);
                }
                else
                {
                    // 只有在不同的分组之间的角色才可以复制，并且不能存在相同节点
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
                            // 设置选中节点
                            SetCurrentSelectNode(newNode);
                        }
                    }
                }
            }

            // 保存分类文件
            // SaveCatelogyFile();
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

            // 查询有没有将父节点移动到子节点的情况
            var parentNode = dropNode.Parent;
            while(parentNode != null)
            {
                if (parentNode == dragNode)
                {
                    return false;
                }
                parentNode = parentNode.Parent;
            }

            // 没有设置Tag的节点不允许移动，只有根节点没有设置
            if (dragNode.Tag == null)
            {
                return false;
            }

            if (dragNode.Tag is TreeViewGroupTag)
            {
                // 移动的节点是分组节点，可以放在根节点，或者其他分组节点上面
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
                // 移动的节点是角色节点，可以放在根节点，或者其他分组节点上面，或者其他角色节点上面（移动TreeView的位置）
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
                // 移动的节点是对话节点，现在不允许移动
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

            RenameGroupForm renameGroupForm = new RenameGroupForm(true);
            if (renameGroupForm.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(renameGroupForm.GroupName))
                {
                    var bInsert = AddNewGroup(renameGroupForm.GroupName);
                    if (bInsert)
                    {
                        TreeNode node = new TreeNode(renameGroupForm.GroupName);
                        TreeViewGroupTag tag = new TreeViewGroupTag()
                        {
                            Name = node.Text
                        };

                        node.Tag = tag;
                        currentNode.Nodes.Add(node);
                    }
                    else
                    {
                        MessageBox.Show("插入分组失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            
        }

        /// <summary>
        /// 保存对话上下文
        /// </summary>
        /// <param name="groupTag"></param>
        /// <param name="actorTag"></param>
        /// <param name="actorNode"></param>
        private void SaveDialogueContext(TreeViewGroupTag groupTag, TreeViewTag actorTag, TreeNode actorNode)
        {

        }

        /// <summary>
        /// 保存编辑过的XML文件
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

                // 第一个层级，是分类
                var tag = node.Tag as TreeViewGroupTag;
                if (tag == null) continue;

                var xGroup = new XElement("category", new XAttribute("name", tag.Name));
                
                
                foreach(TreeNode child in node.Nodes)
                {
                    // 第二个层级，是角色
                    if (child.Tag == null) continue;

                    var tagActor = child.Tag as TreeViewTag;
                    if (tagActor == null) continue;

                    var xActor = new XElement("role", new XAttribute("name", tagActor.Name));
                    xActor.Value = tagActor.Prompt;
                    
                    xGroup.Add(xActor);
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

            var groupTag = groupNode.Tag as TreeViewGroupTag;

            EditActorPromptForm editActorPromptForm = new EditActorPromptForm();
            if (editActorPromptForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(editActorPromptForm.ActorName))
            {
                var newActorId = InsertActor(groupTag.Name, editActorPromptForm.ActorName, editActorPromptForm.Prompt);
                if (newActorId <= 0)
                {
                    MessageBox.Show("插入数据失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                TreeNode treeNode = new TreeNode(editActorPromptForm.ActorName);
                TreeViewTag treeViewTag = new TreeViewTag() { Name = editActorPromptForm.ActorName, Prompt = editActorPromptForm.Prompt, Id = newActorId };
                treeNode.Tag = treeViewTag;

                groupNode.Nodes.Add(treeNode);
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

            if (MessageBox.Show("是否确实要删除该角色", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var actorTag = _CurrentNode.Tag as TreeViewTag;
                // 从数据库中删除
                RemoveActor(actorTag.Id);

                _CurrentNode.Parent.Nodes.Remove(_CurrentNode);
                _CurrentNode = null;
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

                var groupTag = _CurrentNode.Parent.Tag as TreeViewGroupTag;

                UpdateActor(groupTag.Name, tag);
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

            if (MessageBox.Show("是否确定删除当前分组？删除分组以后将删除分组下所有角色","提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // 从数据库中删除分组
                TreeViewGroupTag tag = _CurrentNode.Tag as TreeViewGroupTag;
                RemoveGroup(tag);

                promptTreeView.Nodes.Remove(_CurrentNode);
                _CurrentNode = null;
                // SaveCatelogyFile();
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

            var oldName = tag.Name;

            RenameGroupForm form = new RenameGroupForm();
            form.GroupName = tag.Name;

            if (form.ShowDialog() == DialogResult.OK)
            {
                tag.Name = form.GroupName;
                _CurrentNode.Text = form.GroupName;

                // 更新数据库
                UpdateGroupName(oldName, tag.Name);
            }
        }

        private void 选项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm configForm = new ConfigForm();
            configForm.ShowDialog();

            InitChatGPT();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
                _CurrentNode = null;
            }
        }

        private void renameDialogueMenuItem_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            if (!(_CurrentNode.Tag is TreeViewItemTag))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewItemTag;

            RenameDialogueForm renameDialogueForm = new RenameDialogueForm();
            renameDialogueForm.DialogueName = tag.Name;
            if (renameDialogueForm.ShowDialog() == DialogResult.OK)
            {
                renameDialogue(tag, renameDialogueForm.DialogueName);
                tag.Name = renameDialogueForm.DialogueName;

                _CurrentNode.Text = renameDialogueForm.DialogueName;
            }
        }
    }
}