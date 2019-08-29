using Newtonsoft.Json;

namespace CharismaSDK
{
    public class ConversationResponseParams
    {
        public int ConversationId { get; }
        
        [JsonConstructor]
        public ConversationResponseParams(int conversationId)
        {
            this.ConversationId = conversationId;
        }
    }
}
