using Microsoft.SemanticKernel.AI.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAssistant
{
    internal class LlamaChatCompletion : IChatCompletion
    {
        public LlamaChatCompletion(string url)
        {
            llama_url = url;
            model = "llama3:8b";
        }

        private string llama_url { get; set; }
        private string model { get; set; }

        public ChatHistory CreateNewChat(string? instructions = null)
        {
            return new ChatHistory();
        }

        public async Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
        {
            var sendData = CreateSendData(chat);

            var response = await SendHttpPostRequestWithStream(llama_url, Newtonsoft.Json.JsonConvert.SerializeObject(sendData));

            var result = new List<IChatResult>();
            result.Add(new LLAMA_ChatResult(response));
            return result;
        }

        public IAsyncEnumerable<IChatStreamingResult> GetStreamingChatCompletionsAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private LLAMA_SendData CreateSendData(ChatHistory chatHistory)
        {
            LLAMA_SendData data = new LLAMA_SendData();
            data.model = model;

            foreach (var item in chatHistory)
            {
                var msg = new Message()
                {
                    role = item.Role.ToString().ToLower(),
                    content = item.Content,
                };
                data.messages.Add(msg);
            }
            return data;
        }

        static async Task<string> SendHttpPostRequestWithStream(string url, string postData)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var content = new StringContent(postData, Encoding.UTF8, "application/json"))
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    response.EnsureSuccessStatusCode();

                    var responseText = string.Empty;

                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = await reader.ReadLineAsync();
                                var responseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<LLAMAResponse>(line);
                                responseText += responseMessage.message.content;
                            }
                        }
                    }

                    return responseText;
                }
            }
        }

        public class LLAMA_SendData
        {
            public string model { get; set; }
            public List<Message> messages { get; set; } = new List<Message>();
        }

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }


        public class LLAMAResponse
        {
            public string model { get; set; }
            public string created_at { get; set; }
            public Message message { get; set; }
            public bool done { get; set; }
        }

    }

    class LLAMA_ChatResult : IChatResult
    {
        public LLAMA_ChatResult(string content)
        {
            Content = content;
        }

        private string Content {  get; set; }

        class LLAMA_ChatMessage : ChatMessageBase
        {
            public LLAMA_ChatMessage(string content) : base(AuthorRole.Assistant, content)
            {
            }
        }

        public Task<ChatMessageBase> GetChatMessageAsync(CancellationToken cancellationToken = default)
        {
            ChatMessageBase chatMessage = new LLAMA_ChatMessage(Content);
            return Task.FromResult(chatMessage);
        }
    }
}
