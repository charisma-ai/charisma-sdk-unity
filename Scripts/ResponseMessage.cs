using System.Collections.Generic;
using CharismaPlugin;
using Newtonsoft.Json;

namespace CharismaSDK
{
    /// <summary>
    /// - Contains data related to a characters response such as, text, audio etc.
    /// </summary>
    public class ResponseMessage
    {
        [JsonConstructor]
        public ResponseMessage(string text, Character character, Speech speech,
            Dictionary<string, string> metadata)
        {
            this.Character = character;
            this.Text = text;
            this.Speech = speech;
            this.Metadata = metadata;
        }

        public string Text { get; }
        public Character Character { get; }
        public Speech Speech { get; }
        public Dictionary<string, string> Metadata { get; }
    }
}
