using Newtonsoft.Json;

namespace EMRO.Common.Paubox.Dto
{
    public class Headers
    {
        public string subject { get; set; }
        public string from { get; set; }
    }

    public class Content
    {
        [JsonProperty("text/html")]
        public string textHtml { get; set; }
    }

    public class Message
    {
        public string[] recipients { get; set; }
        public Headers headers { get; set; }
        public Content content { get; set; }
    }

    public class Data
    {
        public Message message { get; set; }
    }

    public class PauboxMain
    {
        public Data data { get; set; }
    }
}
