using System;
using Newtonsoft.Json;

namespace CharismaSDK
{    public class Conversation
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
