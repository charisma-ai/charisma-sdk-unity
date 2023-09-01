using CharismaSDK.Sound;

namespace CharismaSDK.Events
{
    /// <summary>
    /// Options with which to start the interaction with Charisma.
    /// </summary>
    public class StartEvent
    {
        public StartEvent(
            string conversationUuid,
            int? sceneIndex = null,
            string startGraphReferenceId = null,
            SpeechOptions speechConfig = null
        )
        {
            this.conversationUuid = conversationUuid;
            this.sceneIndex = sceneIndex;
            this.startGraphReferenceId = startGraphReferenceId;
            this.speechConfig = speechConfig;
        }

        /// <summary>
        /// UUID of the conversation to start, obtained from `createConversation`.
        /// </summary>
        public string conversationUuid;

        /// <summary>
        /// Index of the scene to start.
        /// </summary>
        public int? sceneIndex = null;

        /// <summary>
        /// Reference ID of the subplot to start.
        /// </summary>
        public string startGraphReferenceId = null;

        /// <summary>
        /// If `speechConfig` is provided, Charisma will return an audio clip of the character's voice alongside the text reply.
        /// </summary>
        public SpeechOptions speechConfig = null;
    }

    public class ReplyEvent
    {
        public string conversationUuid;
        public string text;
        public SpeechOptions speechConfig;

        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to send the reply to.</param>
        /// <param name="text">Message to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.
        ///  - Don't pass unless you want to change settings.'</param>
        public ReplyEvent(string conversationUuid, string text, SpeechOptions speechConfig = null)
        {
            this.conversationUuid = conversationUuid;
            this.text = text;
            this.speechConfig = speechConfig;
        }
    }

    public class TapEvent
    {
        public string conversationUuid;
        public SpeechOptions speechConfig;

        public TapEvent(string conversationUuid, SpeechOptions speechConfig = null)
        {
            this.conversationUuid = conversationUuid;
            this.speechConfig = speechConfig;
        }
    }

    public class ResumeEvent
    {
        /// <summary>
        /// Options with which to resume a playthrough in Charisma.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to resume.</param>
        /// <param name="speechConfig">To use speech, pass speech options.</param>
        public ResumeEvent(string conversationUuid, SpeechOptions speechConfig = null)
        {
            this.conversationUuid = conversationUuid;
            this.speechConfig = speechConfig;
        }

        public string conversationUuid { get; set; }
        public SpeechOptions speechConfig { get; }
    }

    public class ActionEvent
    {
        public string conversationUuid;
        public string action;
        public SpeechOptions speechConfig;

        /// <summary>
        /// Action event, where the player does something non-verbal.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to send the reply to.</param>
        /// <param name="action">The action to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.
        ///  - Don't pass unless you want to change settings.'</param>
        public ActionEvent(string conversationUuid, string action, SpeechOptions speechConfig = null)
        {
            this.conversationUuid = conversationUuid;
            this.action = action;
            this.speechConfig = speechConfig;
        }
    }
}
