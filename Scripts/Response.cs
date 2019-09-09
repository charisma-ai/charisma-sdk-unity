using CharismaPlugin;
using Newtonsoft.Json;

namespace CharismaSDK
{   
    public enum CharismaMessageType
    {
        character,
        media
    }
    
    public class Response
    {
        /// <summary>
        /// - Object returned from charisma.
        /// - Has to be deserialized from Json before being read.
        /// </summary>

        [JsonConstructor]
        public Response(int conversationId, CharismaMessageType messageType, ResponseMessage message, bool endStory, bool tapToContinue, CharacterMood[] characterMoods, Memory[] memories)
        {
            this.ConversationId = conversationId;
            this.CharismaMessageType = messageType;
            this.Message = message;
            this.EndStory = endStory;
            this.CharacterMoods = characterMoods;
            this.TapToContinue = tapToContinue;
            this.Memories = memories;
        }

        public int ConversationId { get; }
        public CharismaMessageType CharismaMessageType { get; }
        public ResponseMessage Message { get; }
        public bool EndStory { get; }
        public bool TapToContinue { get; }
        public CharacterMood[] CharacterMoods { get; }
        public Memory[] Memories { get; }
    }
}
