namespace CharismaSDK
{
    public class PlayerMessage
    {
        public int conversationId { get; }
        public string text { get; }
        public SpeechStartOptions speechConfig { get; set; }
		
        public PlayerMessage(string text, SpeechStartOptions speechConfig, int conversationId)
        {
            this.text = text;
            this.speechConfig = speechConfig;
            this.conversationId = conversationId;
        }	
    }
}