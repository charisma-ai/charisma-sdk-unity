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
    private SpeechOptions _speechOptions = new SpeechOptions();

    [Header(header: "Settings")]
    [SerializeField]
    [Tooltip("Toggle to output messages as audible Speech.")]
    private bool _useSpeechOutput;

    // Singleton pattern
    public static SimplePlaythrough Instance { get; private set; }

    public SpeechOptions SpeechOptions => _speechOptions;
    public bool UseSpeechOutput => _useSpeechOutput;

    private string _conversationUuid;
    private Playthrough _playthrough;

    private CreatePlaythroughTokenParams _playthroughTokenParams;

    private Action _onLoadCallback;

    public event MessageDelegate OnMessage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Destroy any other awakening SimplePlaythroughs
            Destroy(this);
        }
        else
        {
            // Register static Instance
            Instance = this;
        }
    }

    private void Start()
    {
        // Set up default-behaviour callbacks
        // Once load is complete, start the playthrough.
        SetOnLoadCallback(StartPlaythrough);

        // Begin loading playthrough.
        LoadPlaythrough();
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

        // We can now subscribe to message events from charisma.
        _playthrough.OnMessage += ParseMessage;
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

    #endregion

    #region Private functions

    /// <summary>
    /// Standard Message handling behaviour, catching panels and triggering disconnects on endStory.
    /// </summary>
    /// <param name="message">Message Event received from currently active Playthrough.</param>
    private void ParseMessage(MessageEvent message)
    {
        Debug.Log(message);

        // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
        if (message.messageType == MessageType.panel)
        {
            Debug.Log("This is a panel node");
        }
        else
        {
            OnMessage?.Invoke(message);
        }

        // If this is the end of the story, we disconnect from Charisma.
        if (message.endStory)
        {
            _playthrough.Disconnect();
        }
    }

    /// <summary>
    /// Sets the callback to execute on succesfully loading the Playthrough.
    /// Should be set before loading the Playthrough.
    /// </summary>
    private void SetOnLoadCallback(Action callback)
    {
        _onLoadCallback = callback;
    }

    #endregion
}
