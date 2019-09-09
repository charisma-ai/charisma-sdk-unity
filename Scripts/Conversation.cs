using Newtonsoft.Json;

namespace CharismaSDK
{    
    /// <summary>
    /// Instance of a conversation.
    /// </summary>
    public class Conversation
    {
        public int conversationId;
        
        [JsonConstructor]
        public Conversation(int conversationId)
        {
            this.conversationId = conversationId;
        }

        public int ConversationId => conversationId;
    }
}
