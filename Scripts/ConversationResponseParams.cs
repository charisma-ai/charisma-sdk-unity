using Newtonsoft.Json;

namespace CharismaSDK
{
    public class ConversationResponseParams
    {
        /// <summary>
        /// The id of the conversation we have just initialized.
        /// </summary>
        public int ConversationId { get; }
        
        [JsonConstructor]
        public ConversationResponseParams(int conversationId)
        {
            this.ConversationId = conversationId;
        }
    }
}
