using CharismaSDK;
using CharismaSDK.Events;
using System;
using UnityEngine;
using static CharismaSDK.Playthrough;

public class SimplePlaythrough : MonoBehaviour
{
    [Header(header: "Charisma")]
    [SerializeField]
    [Tooltip("Activate Charisma-specific logging.")]
    private bool _enableLogging;

    [SerializeField]
    [Tooltip("Unique ID of the story that you want to play.")]
    private int _storyId;

    [SerializeField]
    [Tooltip("The version of the story you want to play. If set to 0, will load the latest published version. If set to -1, will load the current draft version. The draft also requires the API key to be set")]
    private int _versionId;

    [SerializeField]
    [Tooltip("Used for loading the draft version of the story.")]
    private string _apiKey;

    [SerializeField]
    [Tooltip("Configuration node of the Speech output.")]
    private SpeechOptions _speechOptions = new SpeechOptions(SpeechOptions.AudioOutput.Buffer, SpeechOptions.Encoding.Ogg);

    [Header(header: "Settings")]
    [SerializeField]
    [Tooltip("Toggle to output messages as audible Speech.")]
    private bool _useSpeechOutput;

    [SerializeField]
    [Tooltip("Determines whether the playthrough will be use its own Input/Player and Output/Actor fields (true), or be commanded externally(false). Standalone will auto play the conversation on script activation.")]
    private bool _standalone = true;

    [Header(header: "Input")]
    [SerializeField]
    [Tooltip("Out-of-the-box textbox-based input field. Used if Standalone set to true.")]
    private SimpleCharismaPlayer _player;

    [Header(header: "Output")]
    [SerializeField]
    [Tooltip("Out-of-the-box textbox-based output field. Used if Standalone set to true.")]
    private SimpleCharismaActor _actor;

    [Header(header: "Debug")]
    [SerializeField]
    private ConnectionStateDisplay _display;


    private string _conversationUuid;
    private Playthrough _playthrough;

    private CreatePlaythroughTokenParams _playthroughTokenParams;

    private Action _onLoadCallback;
    private MessageDelegate _onMessageCallback;

    public Playthrough Playthrough => _playthrough;

    private void Start()
    {
        // If _standalone flag is set, auto run playthrough with custom functions
        if (_standalone)
        {
            // Hook up send reply callback, to bind the function to the external player.
            _player.SetOnReplyCallback(SendReply);

            // Set up Actor based on the speech configuration provided within the playthrough.
            _actor.SetSpeechOptions(_speechOptions);
            _actor.SetUseSpeechOutput(_useSpeechOutput);

            // Set up default-behaviour callbacks
            // Once load is complete, start the playthrough.
            SetOnLoadCallback(StartPlaythrough);
            // Hook up standard handle message callback.
            SetOnMessageCallback(DefaultMessageCallback);

            // Begin loading playthrough.
            LoadPlaythrough();
        }
    }


    #region Public Functions

    /// <summary>
    /// Starts the loading process of the Playthrough, by requesting a token and creating a conversation
    /// Will execute OnLoadCallback when complete. 
    /// The OnLoadCallback can be set via the SetLoadCallback function.
    /// </summary>
    public void LoadPlaythrough()
    {
        // We create the config of our token, based on the settings we have defined in the inspector, here.
        _playthroughTokenParams = new CreatePlaythroughTokenParams(_storyId, _versionId, _apiKey);

        // We use these settings to create a play-through token.
        StartCoroutine(CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (tokenResponse) =>
        {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(token: tokenResponse.Token, callback: conversationUuid =>
            {
                // We'll cache our conversation Id since we need this to send replies and other events to Charisma.
                _conversationUuid = conversationUuid;

                // We can now create a new charisma object and pass it our token.
                _playthrough = new Playthrough(
                    token: tokenResponse.Token,
                    playthroughUuid: tokenResponse.PlaythroughUuid,
                    _speechOptions
                );

                _playthrough.OnConnectionStateChange += _display.SetResultState;

                // Invoke any custom user defined action.
                _onLoadCallback?.Invoke();
            }));
        }));
    }

    /// <summary>
    /// Starts the playthrough.
    /// This will begin the relevant active substory, and start sending messages back to the local Unity session.
    /// This message callback can be set via the SetOnMessageCallback function.
    /// </summary>
    public void StartPlaythrough()
    {
        if (!IsLoaded())
        {
            Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
        _playthrough.Connect(() =>
        {
            Debug.Log("Ready!");

            // In the start function, we pass the conversationId we cached earlier. 
            _playthrough.Start(_conversationUuid);
        });

        // On message callback needs to be assigned.
        if (_onMessageCallback != default)
        {
            // We can now subscribe to message events from charisma.
            _playthrough.OnMessage += HandleMessage;
        }
    }

    /// <summary>
    /// Sends a local reply to the Charisma playthrough session.
    /// The playthrough will attempt to send a message in return.
    /// </summary>
    /// <param name="reply"></param>
    public void SendReply(string reply)
    {
        if (!IsLoaded())
        {
            Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // Send a reply to our current conversation.
        _playthrough.Reply(_conversationUuid, reply);
    }

    /// <summary>
    /// Sends an action to the current playthrough.
    /// </summary>
    /// <param name="reply"></param>
    public void SendAction(string action)
    {
        if (!IsLoaded())
        {
            Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // Send an action to our current conversation.
        _playthrough.Action(_conversationUuid, action);
    }

    /// <summary>
    /// Returns whether the Charisma playthrough has succesfully been loaded
    /// </summary>
    public bool IsLoaded()
    {
        return _playthrough != default;
    }

    /// <summary>
    /// Sets the callback to execute on succesfully loading the Playthrough.
    /// Should be set before loading the Playthrough.
    /// </summary>
    public void SetOnLoadCallback(Action callback)
    {
        _onLoadCallback = callback;
    }

    /// <summary>
    /// Sets the callback to execute on succesfully receiving a message from the playthrough.
    /// Should be set before starting the Playthrough.
    /// </summary>
    public void SetOnMessageCallback(MessageDelegate callback)
    {
        _onMessageCallback = callback;
    }

    #endregion

    #region Private functions

    private void DefaultMessageCallback(MessageEvent message)
    {
        _actor.SendMessage(message);
    }

    /// <summary>
    /// Standard Message handling behaviour, catching panels and triggering disconnects on endStory.
    /// </summary>
    /// <param name="message">Message Event received from currently active Playthrough.</param>
    private void HandleMessage(MessageEvent message)
    {
        Debug.Log(message);

        // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
        if (message.messageType == MessageType.panel)
        {
            Debug.Log("This is a panel node");
        }
        else
        {
            _onMessageCallback?.Invoke(message);
        }

        // If this is the end of the story, we disconnect from Charisma.
        if (message.endStory)
        {
            _playthrough.Disconnect();
        }
    }

    #endregion
}
