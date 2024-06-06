using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAssistant
{
    public enum GPT_Version
    {
        None = 0,
        GPT_AZure_3_5 = 1,
        GPT_OpenAI_4 = 2,
        GPT_OLLAMA = 3,
    }

    internal class Global
    {
        public static GPT_Version GPTVersion { get; set; } = GPT_Version.GPT_OLLAMA;
    }
}
