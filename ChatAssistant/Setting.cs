using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChatAssistant
{
    class Setting
    {
        private static Setting _Instance = null;
        public static Setting Instance => _Instance ?? (_Instance = new Setting());

        private Setting() 
        { 
            Load();
        }

        private const string configFile = "App.Config";

        public void Load()
        {
            XElement xDoc = XElement.Load(configFile);
            XElement xSetting = xDoc.Element("setting");

            GPT = new GPTSetting();
            GPT.Load(xSetting.Element("gpt"));
            Proxy = new ProxySetting();
            Proxy.Load(xSetting.Element("proxy"));
            Llama_Url = xSetting.Element("llama")?.Value;
        }

        public void Save()
        {
            XElement xDoc = XElement.Load(configFile);
            XElement xSetting = xDoc.Element("setting");

            xSetting.RemoveNodes();
            xSetting.Add(GPT.Save());
            xSetting.Add(Proxy.Save());
            xSetting.Add(new XElement("llama", Llama_Url));

            xDoc.Save(configFile);
        }

        public GPTSetting GPT { get; set; }
        public ProxySetting Proxy { get; set; }

        public string Llama_Url { get; set; }

        public class GPTSetting
        {

            public void Load(XElement xEle)
            {
                Model = xEle.Attribute("model").Value;
                Key = xEle.Attribute("key").Value;
            }

            public XElement Save()
            {
                return new XElement("gpt", new XAttribute("model", Model)
                    , new XAttribute("key", Key));
            }

            public string Model { get; set; }
            public string Key { get; set; }
        }

        public class ProxySetting
        {
            public void Load(XElement xEle)
            {
                UseProxy = bool.Parse(xEle.Attribute("useProxy").Value);
                ProxyType = xEle.Attribute("proxyType").Value;
                ProxyIp = xEle.Attribute("proxyIp").Value;
                ProxyPort = int.Parse(xEle.Attribute("proxyPort").Value);
            }

            public XElement Save()
            {
                return new XElement("proxy",
                    new XAttribute("useProxy", UseProxy),
                    new XAttribute("proxyType", ProxyType ?? string.Empty),
                    new XAttribute("proxyIp", ProxyIp ?? string.Empty),
                    new XAttribute("proxyPort", ProxyPort));
            }

            public bool UseProxy { get; set; }
            public string ProxyType { get; set; } = string.Empty;
            public string ProxyIp { get; set; } = string.Empty;
            public int ProxyPort { get; set; }
        }
    }
}
