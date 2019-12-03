using System;
using System.Collections.Generic;
using System.Text;

namespace Disbot.Classes
{
    public class SSenseModel
    {
        public Sentiment sentiment { get; set; }
        public Preprocess preprocess { get; set; }
        public object[] alert { get; set; }
        public object[] comparative { get; set; }
        public object[] associative { get; set; }
        public Intention intention { get; set; }
    }
}
