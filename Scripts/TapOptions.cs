namespace CharismaSDK
{
    public class TapOptions
    {
        public int conversationId;
        public SpeechStartOptions speechConfig;

        public TapOptions(Conversation conversation, SpeechStartOptions speechConfig)
        {
            this.conversationId = conversation.ConversationId;
            this.speechConfig = speechConfig;
        }
    }
}
