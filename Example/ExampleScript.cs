using UnityEngine;
using UnityEngine.UI;

using CharismaSDK;
using CharismaSDK.Events;

/// <summary>
/// This script demonstrates a simple interaction with Charisma
/// </summary>
public class ExampleScript : MonoBehaviour
{
    [Header(header: "Charisma")]
    public bool showLog = true;

    public int storyId;
    public int storyVersion;
    public string apiKey;
    [Min(1)] public int startFromScene;

    public bool useSpeech;
    public SpeechOptions speechOptions = new SpeechOptions(SpeechOptions.AudioOutput.Buffer, SpeechOptions.Encoding.Ogg);

    public AudioSource audioSource;

    [Header(header: "UI")]
    public Button button;
    public InputField input;
    public Text text;

    private string _conversationUuid;
    private Playthrough _charisma;

    private void Start()
    {
        // The Charisma logger logs events to and from Charisma.
        CharismaSDK.Logger.logEnabled = showLog;

        // We create the config of our token, based on the settings we have defined in the inspector, here.
        var playthroughTokenParams = new CreatePlaythroughTokenParams(storyId: storyId, storyVersion: storyVersion, apiKey: apiKey);

        // We use these settings to create a play-through token.
        StartCoroutine(CharismaAPI.CreatePlaythroughToken(tokenParams: playthroughTokenParams, callback: (result) =>
        {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(token: result.Token, callback: conversationUuid =>
            {
                // We'll cache our conversation Id since we need this to send replies and other events to Charisma.
                this._conversationUuid = conversationUuid;

                // We can now create a new charisma object and pass it our token.
                this._charisma = new Playthrough(token: result.Token, playthroughUuid: result.PlaythroughUuid);

                // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
                _charisma.Connect(onReadyCallback: () =>
                {
                    Debug.Log("Ready!");

                    // In the start function, we pass the scene we want to start from, the conversationId we cached earlier, and the speech options from the inspector. 
                    _charisma.Start(sceneIndex: startFromScene, conversationUuid: _conversationUuid);
                });

                // We can now subscribe to message events from charisma.
                _charisma.OnMessage += (message) =>
                {
                    Debug.Log(message);
                    // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
                    if (message.messageType == MessageType.panel)
                    {
                        Debug.Log("This is a panel node");

                        // We can't generate speech or access character & text data so we return after we have checked if this is the end of the story.
                        if (message.endStory)
                            _charisma.Disconnect();

                        return;
                    }

                    if (useSpeech && message.message.speech.audio.Length > 0)
                    {
                        // Once we have received a message character message, we might want to play the audio. To do this we run the GetClip method and wait for the callback which contains our audio clip, then pass it to the audio player.
                        Audio.GetAudioClip(speechOptions.encoding, message.message.speech.audio, onAudioGenerated: (clip =>
                        {
                            audioSource.clip = clip;
                            audioSource.Play();
                        }));
                    }

                    text.text = ($"{message.message.character?.name}: {message.message?.text}");

                    // If this is the end of the story, we disconnect from Charisma.
                    if (message.endStory)
                        _charisma.Disconnect();
                };
            }));
        }));

        // Bind the SendPlayerMessage function to the UI button.
        button.onClick.AddListener(call: SendPlayerMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
            SendPlayerMessage();
    }

    public void SendPlayerMessage()
    {
        if (string.IsNullOrEmpty(value: input.text)) return;

        // Send the text of our input field to Charisma.
        _charisma.Reply(conversationUuid: _conversationUuid, message: input.text);

        input.text = string.Empty;
    }
}
