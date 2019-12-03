using Newtonsoft.Json;

namespace Disbot.Classes
{
    public class Sentiment
    {
        public int score { get; set; }
        [JsonProperty("polarity-neg")]
        public bool polarity_neg { get; set; }
        [JsonProperty("polarity-pos")]

        public bool polaryty_pos { get; set; }
        public string polarity { get; set; }
    }
}