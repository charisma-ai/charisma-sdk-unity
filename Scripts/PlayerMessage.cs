namespace CharismaSDK
{  
    public class PlayerMessage
    {
        public int conversationId { get; }
        public string text { get; }
        public SpeechStartOptions speechConfig { get; }
		
        public PlayerMessage(string text, SpeechStartOptions speechConfig, int conversationId)
        {
            this.text = text;
            this.speechConfig = speechConfig;
            this.conversationId = conversationId;
        }	
        
        public PlayerMessage(string text, int conversationId)
        {
            this.text = text;
            this.speechConfig = null;
            this.conversationId = conversationId;
        }	
    }
}