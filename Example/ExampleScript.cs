using UnityEngine;
using UnityEngine.UI;

namespace CharismaSdk.Example
{
    /// <summary>
    /// This script demonstrates a simple interaction with Charisma
    /// </summary>
    public class ExampleScript : MonoBehaviour
    {
        [Header(header: "Charisma")]
        public AudioSource audioSource;
        public string draftToken;
        public bool showLog;
        public int storyId;
        public int storyVersion;
        [Min(1)]public int startFromScene;
        public bool useSpeech;
        public SpeechOptions speechOptions;
        [Header(header: "UI")] 
        public Button button;
        public InputField input;
        public Text text;
        
        private int _conversationId;
        private Charisma _charisma;

        private void Start()
        {
            
            // Before we do anything, we need to set up Charisma. Put this in your initialisation code. You only need to do this one.
            Charisma.Setup();
            
            // The Charisma logger logs events to and from Charisma.
            CharismaLogger.IsActive = showLog;

            // We create the config of our token here, based on the settings we have defined in the inspector.
            var setting = new GetPlaythroughTokenParams(storyId: storyId, storyVersion: storyVersion, draftToken: draftToken);
            
            // We use these settings to create a play-through token.
            Charisma.CreatePlaythroughToken(tokenParams: setting, callback: token =>
            {
                // Once we receive the callback with our token, we can create a new conversation.
                Charisma.CreateConversation(token: token, callback: conversationId =>
                {
                    // We'll cache out conversation Id since we need  this to send replies and other events to Charisma.
                    this._conversationId = conversationId;
                    
                    // We can now create a new charisma object and pass it our token.
                    this._charisma = new Charisma(token: token);

                    // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
                    _charisma.Connect(onReadyCallback: () =>
                    {
                        // In the start function, we pass the scene we want to start from, the conversationId we cached earlier, and the speech options from the inspector. 
                        _charisma.Start(sceneIndex: startFromScene, conversationId: _conversationId, speechOptions: useSpeech? speechOptions : null);
                    });
                    
                    // We can now subscribe to events from charisma.
                    _charisma.OnMessage += (id, message) =>
                    {
                        if (useSpeech)
                        {
                            // Once we have received a message, we want to play the audio. To do this we run the GetClip method and wait for the callback which contains our audio clip, then pass it to the audio player.
                            message.Message.Speech.Audio.GetClip(options: speechOptions, onAudioGenerated: (clip =>
                            {
                                audioSource.clip = clip;
                                audioSource.Play();
                            }));
                        }
                        
                        text.text = ($"{message.Message.Character.Name}: {message.Message.Text}");

                        // If this is the end of the story, we disconnect from Charisma.
                        if(message.EndStory)
                            _charisma.Disconnect();
                    };
                });
            });
            
            // Bind the SendPlayerMessage function to the UI button.
            button.onClick.AddListener(call: SendPlayerMessage);
        }

        private void Update()
        {
            if(Input.GetKeyDown(key: KeyCode.Return))
                SendPlayerMessage();
        }

        public void SendPlayerMessage()
        {
            if(string.IsNullOrEmpty(value: input.text)) return;
            
            // Send the text of our input field to Charisma.
             _charisma.Reply(conversationId: _conversationId, message: input.text);
             
             input.text = string.Empty;
        }
    }
}
