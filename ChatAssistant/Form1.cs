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

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await InitWebView();

            InitChatGPT();
            // InitPrompt();
            InitSqliteDB();
        }

        private IChatCompletion chatGPT = null;
        private IChatCompletion chatCodeX = null;
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
                "***************************",
                "**************************",
                "**************************");
            }

            if (Global.GPTVersion == GPT_Version.GPT_OpenAI_4)
            {
                chatGPT = new OpenAIChatCompletion("**************************",
                    "**************************");
            }

            chatCodeX = new AzureChatCompletion(
                "**************************",
                "**************************",
                "**************************");
        }

        private async Task InitWebView()
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.NavigationCompleted += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(async (sender, e) => await onWebViewNavigationCompleted(sender, e));
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

        private TreeNode? ClearCodePromptTreeNode()
        {
            if (promptTreeView.Nodes.Count > 1)
            {
                var rootNode = promptTreeView.Nodes[1];
                rootNode.Nodes.Clear();
                return rootNode;
            }
            else
            {
                return null;
            }
        }

        class TreeViewActorTag
        {
            public string Prompt { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int Id { get; set; }
            public int ShowOrder { get; set; }
            public bool RecoverContext { get; set; }
        }

        class TreeViewGroupTag
        {
            public string Name { get; set; } = string.Empty;
            public int ShowOrder { get; set; }
        }

        class TreeViewDialogueTag
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

            // ReloadCodeActors(_connection);
            // ReloadCodeDialogues(_connection);
        }

        private void ReloadActors(SqliteConnection conn)
        {
            var rootNode = ClearPromptTreeNode();
            if (rootNode == null)
            {
                return;
            }

            var groups = GetAllGroups(conn);
            var groupOrder = 1;
            foreach(var group in groups)
            {
                TreeNode treeNodeGroup = new TreeNode(group.Name);
                treeNodeGroup.Tag = group;
                rootNode.Nodes.Add(treeNodeGroup);

                var actors = GetGroupActors(group.Name);
                var actorOrder = 1;
                foreach(var actor in actors)
                {
                    TreeNode treeNodeRole = new TreeNode(actor.Name);
                    treeNodeRole.Tag = actor;

                    treeNodeGroup.Nodes.Add(treeNodeRole);
                    ResetActorShowOrderAfterLoad(actor, actorOrder);
                    ResetActorGroupOrderAfterLoad(actor, groupOrder);
                    actorOrder++;
                }
                groupOrder++;
            }

            rootNode.Expand();
        }

        private void ReloadCodeActors(SqliteConnection conn)
        {
            var rootNode = ClearCodePromptTreeNode();
            if (rootNode == null)
            {
                return;
            }

            var groups = GetAllCodeGroups(conn);
            var groupOrder = 1;
            foreach (var group in groups)
            {
                TreeNode treeNodeGroup = new TreeNode(group.Name);
                treeNodeGroup.Tag = group;
                rootNode.Nodes.Add(treeNodeGroup);

                var actors = GetCodeGroupActors(group.Name);
                var actorOrder = 1;
                foreach (var actor in actors)
                {
                    TreeNode treeNodeRole = new TreeNode(actor.Name);
                    treeNodeRole.Tag = actor;

                    treeNodeGroup.Nodes.Add(treeNodeRole);
                    ResetActorShowOrderAfterLoad(actor, actorOrder);
                    ResetActorGroupOrderAfterLoad(actor, groupOrder);
                    actorOrder++;
                }
                groupOrder++;
            }

            rootNode.Expand();
        }

        private void ResetActorShowOrderAfterLoad(TreeViewActorTag actor, int order)
        {
            var command = _connection.CreateCommand();

            command.CommandText = $"update actor set actororder={order} where id={actor.Id}";
            command.ExecuteNonQuery();
        }

        private void ResetActorGroupOrderAfterLoad(TreeViewActorTag actor, int order)
        {
            var command = _connection.CreateCommand();

            command.CommandText = $"update actor set grouporder={order} where id={actor.Id}";
            command.ExecuteNonQuery();
        }

        private void ReloadDialogues(SqliteConnection conn)
        {
            var command = conn.CreateCommand();
            // ȡ�����жԻ�
            command.CommandText = @"select id, actorid, name from dialogue";
            List<TreeViewDialogueTag> itemTagList = new List<TreeViewDialogueTag>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    if (reader.IsDBNull(1) || reader.IsDBNull(2)) continue;

                    var actorid = reader.GetInt32(1);
                    var name = reader.GetString(2);

                    TreeViewDialogueTag itemTag = new TreeViewDialogueTag()
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

        private void ReloadCodeDialogues(SqliteConnection conn)
        {
            var command = conn.CreateCommand();
            // ȡ�����жԻ�
            command.CommandText = @"select id, actorid, name from dialogue";
            List<TreeViewDialogueTag> itemTagList = new List<TreeViewDialogueTag>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    if (reader.IsDBNull(1) || reader.IsDBNull(2)) continue;

                    var actorid = reader.GetInt32(1);
                    var name = reader.GetString(2);

                    TreeViewDialogueTag itemTag = new TreeViewDialogueTag()
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

        private void ReloadDialogContext(List<TreeViewDialogueTag> itemTagList)
        {
            var rootNode = promptTreeView.Nodes[0];
            foreach(TreeNode groupNode in rootNode.Nodes)
            {
                TreeViewGroupTag treeViewGroupTag = groupNode.Tag as TreeViewGroupTag;
                if (treeViewGroupTag == null) continue;

                foreach(TreeNode actorNode in groupNode.Nodes)
                {
                    TreeViewActorTag actorTag = actorNode.Tag as TreeViewActorTag;
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

        private void RebuildDialogueNode(ChatHistory chatMessages, TreeViewDialogueTag tagDialogue, TreeNode actorNode)
        {
            // ����ϵͳ��ʾ��
            TreeViewActorTag tagActor = actorNode.Tag as TreeViewActorTag;
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

        private int InsertChatMessage(int dialogueId, string roleId, string message)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO \"chatmessage\"(\"dialogueid\", \"role\", \"message\") VALUES(@dialogueid, @role, @message); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@dialogueid", dialogueId);
            cmd.Parameters.AddWithValue("@role", roleId);
            cmd.Parameters.AddWithValue("@message", message);

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

        private void UpdateChatMessage(int dialogueId, string roleId, string message)
        {
            try
            {
                var cmd = _connection.CreateCommand();
                cmd.CommandText = $"select max(id) from chatmessage where dialogueid={dialogueId} and role=@role";
                cmd.Parameters.AddWithValue("@role", roleId);
                using (var reader = cmd.ExecuteReader())
                {
                    var id = 0;
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                        break;
                    }
                    reader.Close();

                    if (id != 0)
                    {
                        cmd.CommandText = $"update chatmessage set message=@message where id={id}";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@message", message);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void renameDialogue(TreeViewDialogueTag itemTag, string newName)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update dialogue set name=@name where id=@id";
            cmd.Parameters.AddWithValue("@name", newName);
            cmd.Parameters.AddWithValue("@id", itemTag.Id);

            cmd.ExecuteNonQuery();
        }

        private List<TreeViewGroupTag> GetAllGroups(SqliteConnection conn)
        {
            List<TreeViewGroupTag> groups = new List<TreeViewGroupTag>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT \"group\", grouporder FROM \"actor\"";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var group = reader.GetString(0);
                    if (reader.IsDBNull(1)) continue;

                    var groupOrder = reader.GetInt32(1);

                    TreeViewGroupTag tag = new TreeViewGroupTag() { Name = group, ShowOrder = groupOrder };
                    groups.Add(tag);
                }
            }
            groups.Sort((a, b) => a.ShowOrder.CompareTo(b.ShowOrder));
            return groups;
        }

        private List<TreeViewGroupTag> GetAllCodeGroups(SqliteConnection conn)
        {
            List<TreeViewGroupTag> groups = new List<TreeViewGroupTag>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT \"group\", grouporder FROM \"actorProgram\"";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var group = reader.GetString(0);
                    if (reader.IsDBNull(1)) continue;

                    var groupOrder = reader.GetInt32(1);

                    TreeViewGroupTag tag = new TreeViewGroupTag() { Name = group, ShowOrder = groupOrder };
                    groups.Add(tag);
                }
            }
            groups.Sort((a, b) => a.ShowOrder.CompareTo(b.ShowOrder));
            return groups;
        }

        private List<TreeViewActorTag> GetGroupActors(string groupName)
        {
            List<TreeViewActorTag> results = new List<TreeViewActorTag>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, actor, actororder, prompt, recovercontext FROM \"actor\" where \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", groupName);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeViewActorTag tag = new TreeViewActorTag();
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
                        tag.ShowOrder = reader.GetInt32(2);
                    }

                    if (!reader.IsDBNull(3))
                    {
                        tag.Prompt = reader.GetString(3);
                    }
                    
                    if (!reader.IsDBNull(4))
                    {
                        tag.RecoverContext = reader.GetBoolean(4);
                    }

                    results.Add(tag);
                }
            }

            results.Sort((a, b) => a.ShowOrder.CompareTo(b.ShowOrder));

            return results;
        }

        private List<TreeViewActorTag> GetCodeGroupActors(string groupName)
        {
            List<TreeViewActorTag> results = new List<TreeViewActorTag>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, actor, actororder, prompt, recovercontext FROM \"actorProgram\" where \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", groupName);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeViewActorTag tag = new TreeViewActorTag();
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
                        tag.ShowOrder = reader.GetInt32(2);
                    }

                    if (!reader.IsDBNull(3))
                    {
                        tag.Prompt = reader.GetString(3);
                    }

                    if (!reader.IsDBNull(4))
                    {
                        tag.RecoverContext = reader.GetBoolean(4);
                    }

                    results.Add(tag);
                }
            }

            results.Sort((a, b) => a.ShowOrder.CompareTo(b.ShowOrder));

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

        private void UpdateActor(string groupName, TreeViewActorTag actorTag)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update actor set \"group\"=@group, actor=@actor, prompt=@prompt, recovercontext=@recovercontext where id=@id";
            cmd.Parameters.AddWithValue("@group", groupName);
            cmd.Parameters.AddWithValue("@actor", actorTag.Name);
            cmd.Parameters.AddWithValue("@prompt", actorTag.Prompt);
            cmd.Parameters.AddWithValue("@id", actorTag.Id);
            cmd.Parameters.AddWithValue("@recovercontext", actorTag.RecoverContext);

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

        private void ResetActorGroupName(int actorId, string newName, int actorOrder)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "update actor set \"group\"=@group, grouporder=@grouporder where id=@id";
            cmd.Parameters.AddWithValue("@group", newName);
            cmd.Parameters.AddWithValue("@grouporder", actorOrder);
            cmd.Parameters.AddWithValue("@id", actorId);

            cmd.ExecuteNonQuery();
        }

        //private void ResetActorShowOrder(TreeViewActorTag actor, int newOrder)
        //{
        //    var oldOrder = actor.ShowOrder;
        //    if (oldOrder == newOrder) return;
        //    var cmd = _connection.CreateCommand();
        //    if (oldOrder < newOrder)
        //    {
        //        cmd.CommandText = $"UPDATE actor SET actororder = actororder + 1 WHERE actororder >= {newOrder}";
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET actororder = {newOrder + 1} WHERE id = {actor.Id}";
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET actororder = actororder - 1 WHERE actororder >= {oldOrder}";
        //        cmd.ExecuteNonQuery();
        //    }
        //    else
        //    {
        //        // UPDATE ���� SET ShowOrder = ShowOrder + 1 WHERE ShowOrder >= 3 AND ShowOrder <= 5;
        //        cmd.CommandText = $"UPDATE actor SET actororder = actororder + 1 WHERE actororder >= {newOrder} AND actororder < {oldOrder}";
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET actororder = {newOrder} WHERE id = {actor.Id}";
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        //private void ResetGroupShowOrder(TreeViewGroupTag group, int newOrder)
        //{
        //    var oldOrder = group.ShowOrder;
        //    var cmd = _connection.CreateCommand();
        //    if (oldOrder < newOrder)
        //    {
        //        cmd.CommandText = $"UPDATE actor SET grouporder = grouporder + 1 WHERE grouporder >= {newOrder}";
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET grouporder = {newOrder} WHERE \"group\"=@group";
        //        cmd.Parameters.AddWithValue("@group", group.Name);
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET grouporder = grouporder - 1 WHERE grouporder >= {group.ShowOrder} AND actororder <= {newOrder} AND \"group\"!=@group";
        //        cmd.ExecuteNonQuery();
        //    }
        //    else if (oldOrder > newOrder)
        //    {
        //        cmd.CommandText = $"UPDATE actor SET grouporder = grouporder + 1 WHERE grouporder >= {newOrder} AND grouporder < {oldOrder}";
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = $"UPDATE actor SET grouporder = {newOrder} WHERE \"group\"=@group";
        //        cmd.Parameters.AddWithValue("@group", group.Name);
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        private void ResetActorShowOrder(TreeViewActorTag actor, int newOrder)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"UPDATE actor SET actororder = {newOrder} WHERE id = {actor.Id}";
            cmd.ExecuteNonQuery();
        }

        private void ResetGroupShowOrder(TreeViewGroupTag group, int newOrder)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"UPDATE actor SET grouporder = {newOrder} WHERE \"group\"=@group";
            cmd.Parameters.AddWithValue("@group", group.Name);
            cmd.ExecuteNonQuery();
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
                        TreeViewActorTag roleTag = new TreeViewActorTag() { Name = roleName, Prompt = rolePrompt };
                        treeNodeRole.Tag = roleTag;

                        treeNodeGroup.Nodes.Add(treeNodeRole);
                    }
                }
            }
            catch (Exception) { }

            rootNode.Expand();
        }

        private async void promptTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            await ShowTreeNode(e.Node);
        }

        private async Task ShowTreeNode(TreeNode node)
        {
            var selectNode = node;
            if (selectNode == null || selectNode.Tag == null)
            {
                _CurrentNode = null;
                return;
            }

            var treeViewTag = selectNode.Tag as TreeViewActorTag;
            if (treeViewTag != null)
            {
                _CurrentNode = selectNode;
                ShowTreeViewTag(treeViewTag);
            }
            else if (selectNode.Tag is TreeViewDialogueTag)
            {
                TreeViewActorTag actorTag = selectNode.Parent.Tag as TreeViewActorTag;
                TreeViewDialogueTag itemTag = selectNode.Tag as TreeViewDialogueTag;
                _CurrentNode = selectNode;
                await ShowTreeViewTag(itemTag, actorTag);
            }
            else if (selectNode.Tag is TreeViewGroupTag)
            {

            }
        }

        /// <summary>
        /// ���ұߵ�TextBox����ʾTag������
        /// </summary>
        /// <param name="tag"></param>
        private void ShowTreeViewTag(TreeViewActorTag actor)
        {
            StringBuilder sb = new StringBuilder();
            // ����
            sb.Append("<html>");
            sb.Append("<body>");
            // sb.AppendLine($"<p>[{tag.Prompt}]<p>");
            sb.AppendLine($"<p>���ã�����һ��\"{actor.Name}\", ��������ʲô�����ҵ���</p>");
            sb.AppendLine("<br/>");
            sb.AppendLine("</body></html>");

            webView21.NavigateToString(sb.ToString());
        }

        /// <summary>
        /// ���ұߵ�TextBox����ʾTag������
        /// </summary>
        /// <param name="tag"></param>
        private async Task ShowTreeViewTag(TreeViewDialogueTag tag, TreeViewActorTag actor)
        {
            //StringBuilder sb = new StringBuilder();
            //// ����
            //sb.Append("<html>");
            //sb.Append("<body>");
            //// sb.AppendLine($"<p>[{tag.Prompt}]<p>");
            //sb.AppendLine($"<p>���ã�����һ��\"{actor.Name}\", ��������ʲô�����ҵ���</p>");
            //sb.AppendLine("<br/>");
            var header = $"���ã�����һ��\"{actor.Name}\", ��������ʲô�����ҵ���";

            List<string> showContents = new List<string>();

            // ����ʷ�е�Message����
            foreach (var message in tag.Chat_History)
            {
                if (message.Role == AuthorRole.System)
                {
                    continue;
                }

                if (message.Role == AuthorRole.User)
                {
                    // sb.Append("<p>Q : ");
                    // sb.AppendLine($"{message.Content}</p>");
                    // sb.AppendLine("<br/>");
                    showContents.Add($"Q : {message.Content}");
                }

                if (message.Role == AuthorRole.Assistant)
                {
                    // sb.Append("<p>A : ");
                    // sb.AppendLine($"{message.Content}</p>");
                    // sb.AppendLine("<br/>");
                    showContents.Add($"A : {message.Content}");
                }
            }

            webView21.CoreWebView2.Navigate($"file:///{Environment.CurrentDirectory}/ShowCode.html");

            _ExecuteContentScript = $"showContentArray(`{header}`, {Newtonsoft.Json.JsonConvert.SerializeObject(showContents)})";
        }

        private string _ExecuteContentScript = string.Empty;

        private async Task onWebViewNavigationCompleted(object objSender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_ExecuteContentScript))
            {
                await webView21.ExecuteScriptAsync(_ExecuteContentScript);
                _ExecuteContentScript = string.Empty;
            }
        }

        private IChatCompletion GetCurrentChatEngine()
        {
            if (_CurrentNode == null || _CurrentNode.Parent == null)
            {
                return null;
            }

            if (promptTreeView.Nodes.Count <= 1)
            {
                return chatGPT;
            }

            TreeNode nodeParent = _CurrentNode.Parent;
            while(nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            if (nodeParent.Name == promptTreeView.Nodes[1].Name)
            {
                return chatCodeX;
            }
            else
            {
                return chatGPT;
            }
        }

        private async Task ChatMessage(TreeViewDialogueTag tag, TreeViewActorTag actor, string chatMessage = null)
        {
            if (chatMessage == null)
            {
                tag.Chat_History.AddUserMessage(tb_UserMessage.Text);
                chatMessage = tb_UserMessage.Text;

            }
            else
            {
                tag.Chat_History.AddUserMessage(chatMessage);
            }

            // ����Ի�
            if (chatMessage != "continue" && chatMessage != "����")
            {
                InsertChatMessage(tag.Id, AuthorRole.User.Label, chatMessage);
                await ShowTreeViewTag(tag, actor);
            }

            var reply = string.Empty;
            var chatEngine = GetCurrentChatEngine();

            if (actor.RecoverContext)
            {
                tb_UserMessage.Text = String.Empty;
                ChatRequestSettings chatRequestSettings = new ChatRequestSettings();
                chatRequestSettings.MaxTokens = 2048;
                
                reply = await chatEngine.GenerateMessageAsync(tag.Chat_History, chatRequestSettings);
            }
            else
            {
                tb_UserMessage.Text = String.Empty;
                ChatRequestSettings chatRequestSettings = new ChatRequestSettings();
                chatRequestSettings.MaxTokens = 2048;

                ChatHistory newChat = new ChatHistory();
                newChat.AddSystemMessage(actor.Prompt);
                newChat.AddUserMessage(chatMessage);

                reply = await chatEngine.GenerateMessageAsync(newChat, chatRequestSettings);
            }

            if (chatMessage == "continue" || chatMessage == "����")
            {
                var last = tag.Chat_History.Last();
                if (last.Content == chatMessage)
                {
                    tag.Chat_History.Remove(last);
                }

                last = tag.Chat_History.FindLast(x => x.Role.Label == AuthorRole.Assistant.Label);
                if (last != null)
                {
                    last.Content = last.Content + reply;

                    UpdateChatMessage(tag.Id, AuthorRole.Assistant.Label, last.Content);
                }
                else
                {
                    tag.Chat_History.AddAssistantMessage(reply);
                    InsertChatMessage(tag.Id, AuthorRole.Assistant.Label, reply);
                }

                await ShowTreeViewTag(tag, actor);
            }
            else
            {
                tag.Chat_History.AddAssistantMessage(reply);

                await ShowTreeViewTag(tag, actor);
                // ����Ի�
                InsertChatMessage(tag.Id, AuthorRole.Assistant.Label, reply);
            }
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

            var tag = _CurrentNode.Tag as TreeViewActorTag;
            if (tag != null)
            {
                var dialogueCount = _CurrentNode.Nodes.Count;
                var name = $"�Ի�{dialogueCount + 1}";
                var dialogueId = InsertNewDialogue(tag.Id, name);
                if (dialogueId <= 0)
                {
                    MessageBox.Show("����Ի�ʧ�ܣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // �½�һ����Item
                TreeNode node = new TreeNode(name);
                TreeViewDialogueTag itemTag = new TreeViewDialogueTag()
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

                await ChatMessage(itemTag, tag);
            }
            else
            {
                var itemTag = _CurrentNode.Tag as TreeViewDialogueTag;
                var actorTag = _CurrentNode.Parent.Tag as TreeViewActorTag;
                if (itemTag != null)
                {
                    await ChatMessage(itemTag, actorTag);
                }
            }
        }

        private void NewDialogueMenuItem_Click(object sender, EventArgs e)
        {
            TreeViewActorTag dialogueTag = null;
            TreeNode actorNode = null;

            if (_CurrentNode == null || _CurrentNode.Tag == null)
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewActorTag;
            if (tag != null)
            {
                dialogueTag = tag;
                actorNode = _CurrentNode;
            }

            var oldItemTag = _CurrentNode.Tag as TreeViewDialogueTag;
            if (oldItemTag != null)
            {
                var parentNode = _CurrentNode.Parent as TreeNode;
                dialogueTag = parentNode.Tag as TreeViewActorTag;
                actorNode = parentNode;
            }

            if (dialogueTag == null)
            {
                return;
            }

            var dialogueCount = actorNode.Nodes.Count;
            var name = $"�Ի�{dialogueCount + 1}";
            var dialogueId = InsertNewDialogue(tag.Id, name);
            if (dialogueId <= 0)
            {
                MessageBox.Show("����Ի�ʧ�ܣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // �½�һ����Item
            TreeNode node = new TreeNode(name);
            TreeViewDialogueTag itemTag = new TreeViewDialogueTag()
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
                renameDialogueMenuItem.Visible = false;
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
                renameDialogueMenuItem.Visible = false;
            }
            else if (tag is TreeViewActorTag)
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
                renameDialogueMenuItem.Visible = false;
            }
            else if (tag is TreeViewDialogueTag)
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
                renameDialogueMenuItem.Visible = true;
            }
        }

        private void promptTreeView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private async void promptTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            // �������Ч������Ҽ�����������������Ҽ�����ڵ�Ϊѡ��״̬
            if (lastClickNode != null)
            {
                promptTreeView.SelectedNode = lastClickNode;
                lastClickNode.Checked = true;
                _CurrentNode = lastClickNode;

                // await ShowTreeNode(_CurrentNode);
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

        private void ResetAllActorOrder(TreeNode node)
        {
            if (node.Tag == null || !(node.Tag is TreeViewGroupTag))
            {
                return;
            }

            var index = 1;
            foreach(TreeNode actorNode in node.Nodes)
            {
                TreeViewActorTag tag = actorNode.Tag as TreeViewActorTag;
                if (tag == null) continue;
                tag.ShowOrder = index;

                ResetActorShowOrder(tag, index);

                index++;
            }
        }

        private void ResetAllGroupOrder()
        {
            var rootNode = promptTreeView.Nodes[0];
            var index = 1;
            foreach (TreeNode groupNode in rootNode.Nodes)
            {
                TreeViewGroupTag tag = groupNode.Tag as TreeViewGroupTag;
                if (tag == null) continue;
                tag.ShowOrder = index;

                ResetGroupShowOrder(tag, index);

                index++;
            }
        }

        private void AfterDragNode_MoveNode(TreeNode node, int newMoveIndex, TreeNode oldParent)
        {
            if (oldParent == null)
            {
                if (node.Tag is TreeViewActorTag)
                {
                    ResetAllActorOrder(node);
                }
                else if (node.Tag is TreeViewGroupTag)
                {
                    ResetAllGroupOrder();
                }
            }
            else
            {
                if (node.Tag is TreeViewActorTag)
                {
                    TreeViewActorTag actor = node.Tag as TreeViewActorTag;
                    TreeViewGroupTag group = node.Parent.Tag as TreeViewGroupTag;

                    ResetActorGroupName(actor.Id, group.Name, group.ShowOrder);

                    ResetAllActorOrder(node.Parent);
                    ResetAllActorOrder(oldParent);
                }
            }
        }

        private void AfterDragNode_CopyNode(TreeNode node, int newMoveIndex)
        {
            TreeViewActorTag actor = node.Tag as TreeViewActorTag;
            TreeViewGroupTag group = node.Parent.Tag as TreeViewGroupTag;

            // ��������
            var newActorId = InsertActor(group.Name, actor.Name, actor.Prompt);
            actor.Id = newActorId;

            ResetActorGroupName(actor.Id, group.Name, group.ShowOrder);

            ResetAllActorOrder(node.Parent);
        }

        //private void AfterDragNode_MoveNode(TreeNode node, int newMoveIndex, TreeNode oldParent)
        //{
        //    if (oldParent == null)
        //    {
        //        // ���Լ�����������ק
        //        if (node.Tag is TreeViewActorTag)
        //        {
        //            var actor = node.Tag as TreeViewActorTag;

        //            ResetActorShowOrder(actor, newMoveIndex);
        //            // ���½�����TreeNode����
        //            ResetAllActorOrder(node.Parent);
        //        }
        //        else if (node.Tag is TreeViewGroupTag)
        //        {
        //            var group = node.Tag as TreeViewGroupTag;

        //            ResetGroupShowOrder(group, newMoveIndex);
        //            // ���½�����TreeNode����
        //            ResetAllGroupOrder();
        //        }
        //        else if (node.Tag is TreeViewDialogueTag)
        //        {
        //            var dialogue = node.Tag as TreeViewDialogueTag;

        //            // ���½�����TreeNode����
        //        }
        //    }
        //    else
        //    {
        //        if (node.Tag is TreeViewActorTag)
        //        {
        //            var actor = node.Tag as TreeViewActorTag;
        //            // ���½�ɫNode�������ƣ�����ŵ�����λ��
        //            var lastOrder = node.Parent.Nodes.Count + 1;
        //            var groupTag = node.Parent.Tag as TreeViewGroupTag;
        //            ResetActorGroupName(actor.Id, groupTag.Name, lastOrder);

        //            // ����λ��
        //            ResetActorShowOrder(actor, newMoveIndex);
        //            // ���½�����TreeNode����
        //            ResetAllActorOrder(node.Parent);
        //            ResetAllActorOrder(oldParent);
        //        }
        //        else if (node.Tag is TreeViewGroupTag)
        //        {

        //        }
        //    }
        //}

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
            if (dragNode == null || dragNode.Parent == null)
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
                    AfterDragNode_MoveNode(dragNode, 1, null);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    var newIndex = targetNode.Index + 1;
                    // �ƶ����ڵ������λ��
                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(newIndex, dragNode);

                    AfterDragNode_MoveNode(dragNode, newIndex + 1, null);
                }
                else
                {
                    //��һ�������ƶ�������һ���������� ��Ҫ��֤�����������ڵ�
                    var findNode = FindSubNodeByName(targetNode.Parent, dragNode.Text);
                    if (findNode == null)
                    {
                        if (targetNode.Parent != null)
                        {
                            var newIndex = targetNode.Index + 1;
                            // ��ԭ���ĸ��ڵ���ɾ�����ƶ��Ľڵ�
                            var oldParent = dragNode.Parent;
                            oldParent.Nodes.Remove(dragNode);

                            // Ŀ��ڵ����Ϊ�ӽڵ�
                            targetNode.Parent.Nodes.Insert(newIndex, dragNode);
                            AfterDragNode_MoveNode(dragNode, newIndex + 1, oldParent);
                        }
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

                    AfterDragNode_MoveNode(dragNode, 1, null);
                    // ����ѡ�нڵ�
                    SetCurrentSelectNode(dragNode);
                }
                else if (dragNode.Parent == targetNode.Parent)
                {
                    // �ƶ����ڵ������λ��
                    var newIndex = targetNode.Index + 1;
                    targetNode.Parent.Nodes.Remove(dragNode);
                    targetNode.Parent.Nodes.Insert(newIndex, dragNode);

                    AfterDragNode_MoveNode(dragNode, newIndex, null);

                    // ����ѡ�нڵ�
                    SetCurrentSelectNode(dragNode);
                }
                else
                {
                    // ֻ���ڲ�ͬ�ķ���֮��Ľ�ɫ�ſ��Ը��ƣ����Ҳ��ܴ�����ͬ�ڵ�
                    var findNode = FindSubNodeByName(targetNode.Parent, dragNode.Text);
                    if (findNode == null)
                    {
                        if (targetNode.Parent != null)
                        {
                            var tagDrag = dragNode.Tag as TreeViewActorTag;
                            var tagDrop = targetNode.Tag as TreeViewActorTag;
                            if (tagDrag != null && tagDrop != null)
                            {
                                TreeNode newNode = new TreeNode(tagDrag.Name);
                                newNode.Tag = tagDrag;

                                targetNode.Parent.Nodes.Insert(targetNode.Index + 1, newNode);

                                AfterDragNode_CopyNode(newNode, targetNode.Index + 1);

                                // ����ѡ�нڵ�
                                SetCurrentSelectNode(newNode);
                            }
                        }
                    }
                }
            }
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

            if (dragNode.Tag is TreeViewActorTag)
            {
                // �ƶ��Ľڵ��ǽ�ɫ�ڵ㣬���Է��ڸ��ڵ㣬������������ڵ����棬����������ɫ�ڵ����棨�ƶ�TreeView��λ�ã�
                if (dropNode.Tag == null || dropNode.Tag is TreeViewGroupTag || dropNode.Tag is TreeViewActorTag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (dragNode.Tag is TreeViewDialogueTag)
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
                        MessageBox.Show("�������ʧ�ܣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            
        }

        /// <summary>
        /// ����Ի�������
        /// </summary>
        /// <param name="groupTag"></param>
        /// <param name="actorTag"></param>
        /// <param name="actorNode"></param>
        private void SaveDialogueContext(TreeViewGroupTag groupTag, TreeViewActorTag actorTag, TreeNode actorNode)
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

                    var tagActor = child.Tag as TreeViewActorTag;
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

            if (_CurrentNode.Tag is TreeViewActorTag)
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
                    MessageBox.Show("��������ʧ�ܣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                TreeNode treeNode = new TreeNode(editActorPromptForm.ActorName);
                TreeViewActorTag treeViewTag = new TreeViewActorTag() { Name = editActorPromptForm.ActorName, Prompt = editActorPromptForm.Prompt, Id = newActorId };
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

            if (!(_CurrentNode.Tag is TreeViewActorTag))
            {
                return;
            }

            if (MessageBox.Show("�Ƿ�ȷʵҪɾ���ý�ɫ", "��ʾ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var actorTag = _CurrentNode.Tag as TreeViewActorTag;
                // �����ݿ���ɾ��
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

            if (!(_CurrentNode.Tag is TreeViewActorTag))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewActorTag;

            EditActorPromptForm editActorPromptForm = new EditActorPromptForm();
            editActorPromptForm.Init(tag.Name, tag.Prompt, tag.RecoverContext);

            if (editActorPromptForm.ShowDialog() == DialogResult.OK)
            {
                _CurrentNode.Text = editActorPromptForm.ActorName;
                tag.Name = editActorPromptForm.ActorName;
                tag.Prompt = editActorPromptForm.Prompt;
                tag.RecoverContext = editActorPromptForm.RecoverContext;

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

            if (MessageBox.Show("�Ƿ�ȷ��ɾ����ǰ���飿ɾ�������Ժ�ɾ�����������н�ɫ","��ʾ", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // �����ݿ���ɾ������
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

                // �������ݿ�
                UpdateGroupName(oldName, tag.Name);
            }
        }

        private void ѡ��ToolStripMenuItem_Click(object sender, EventArgs e)
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

            if (!(_CurrentNode.Tag is TreeViewDialogueTag))
            {
                return;
            }

            var tag = _CurrentNode.Tag as TreeViewDialogueTag;

            RenameDialogueForm renameDialogueForm = new RenameDialogueForm();
            renameDialogueForm.DialogueName = tag.Name;
            if (renameDialogueForm.ShowDialog() == DialogResult.OK)
            {
                renameDialogue(tag, renameDialogueForm.DialogueName);
                tag.Name = renameDialogueForm.DialogueName;

                _CurrentNode.Text = renameDialogueForm.DialogueName;
            }
        }

        private async void btnContinue_Click(object sender, EventArgs e)
        {
            if (_CurrentNode == null)
            {
                return;
            }

            var itemTag = _CurrentNode.Tag as TreeViewDialogueTag;
            if (itemTag == null)
            {
                return;
            }
            var actorTag = _CurrentNode.Parent.Tag as TreeViewActorTag;
            if (actorTag == null)
            {
                return;
            }

            await ChatMessage(itemTag, actorTag, "continue");
        }
    }
}