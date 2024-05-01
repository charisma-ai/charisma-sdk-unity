using CharismaSDK.Sound;

namespace CharismaSDK.Events
{
    public abstract class ClientEvent
    {
        public string conversationUuid;
        public SpeechOptions speechConfig = null;

        protected ClientEvent(string conversationUuid, SpeechOptions speechConfig = null)
        {
            this.conversationUuid = conversationUuid;
            this.speechConfig = speechConfig;
        }
    }

    /// <summary>
    /// Options with which to start the interaction with Charisma.
    /// </summary>
    public class StartEvent : ClientEvent
    {
        public int? sceneIndex = null;
        public string startGraphReferenceId = null;

        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to send the reply to.</param>
        /// <param name="sceneIndex">Index of scene to start the playthrough from</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.- Don't pass unless you want to change settings.</param>
        /// /// <param name="startGraphReferenceId">Graph sublot to start the playthrough from</param>
        public StartEvent(string conversationUuid, SpeechOptions speechConfig = null,  int? sceneIndex = null, string startGraphReferenceId = null) : base(conversationUuid, speechConfig)
        {
            this.sceneIndex = sceneIndex;
            this.startGraphReferenceId = startGraphReferenceId;
        }
    }

    public class ReplyEvent : ClientEvent
    {
        public string text;

        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to send the reply to.</param>
        /// <param name="text">Message to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.- Don't pass unless you want to change settings.</param>
        public ReplyEvent(string conversationUuid, string text, SpeechOptions speechConfig = null) : base(conversationUuid, speechConfig)
        {
            this.text = text;
        }
    }

    public class TapEvent : ClientEvent
    {
        public TapEvent(string conversationUuid, SpeechOptions speechConfig = null) : base(conversationUuid, speechConfig)
        {
        }
    }

    public class ResumeEvent : ClientEvent
    {
        /// <summary>
        /// Options with which to resume a playthrough in Charisma.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to resume.</param>
        /// <param name="speechConfig">To use speech, pass speech options.</param>
        public ResumeEvent(string conversationUuid, SpeechOptions speechConfig = null) : base(conversationUuid, speechConfig)
        {
        }
    }

    public class ActionEvent : ClientEvent
    {
        public string action;

        /// <summary>
        /// Action event, where the player does something non-verbal.
        /// </summary>
        /// <param name="conversationUuid">Id of the conversation to send the reply to.</param>
        /// <param name="action">The action to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.
        ///  - Don't pass unless you want to change settings.'</param>
        public ActionEvent(string conversationUuid, string action, SpeechOptions speechConfig = null) : base(conversationUuid, speechConfig)
        {
            this.action = action;
        }
    }
}
