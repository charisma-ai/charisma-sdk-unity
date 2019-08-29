namespace CharismaSDK
{
    public class CharismaStartOptions
    {
        public CharismaStartOptions(int conversationId, int sceneIndex, SpeechStartOptions speechConfig)
        {
            this.conversationId = conversationId;
            this.sceneIndex = sceneIndex;
            this.speechConfig = speechConfig;			
        }
        
        
        public CharismaStartOptions(int conversationId, int sceneIndex)
        {
            this.conversationId = conversationId;
            this.sceneIndex = sceneIndex;
            this.speechConfig = null;			
        }

        public int conversationId { get; set; }
        public int sceneIndex { get; }
        public SpeechStartOptions speechConfig { get; }
    }
}
