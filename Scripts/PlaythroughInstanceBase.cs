using System;
using CharismaSDK;
using CharismaSDK.Audio;
using CharismaSDK.Events;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = CharismaSDK.Logger;

public abstract class PlaythroughInstanceBase : MonoBehaviour
{
    [Header("Charisma")]
    [SerializeField]
    [Tooltip("Unique ID of the story that you want to play.")]
    protected int _storyId;

    [SerializeField]
    [Tooltip("The version of the story you want to play. If set to 0, will load the latest published version. If set to -1, will load the current draft version. The draft also requires the API key to be set")]
    protected int _storyVersion;

    [SerializeField]
    [Tooltip("Used for loading the draft version of the story.")]
    protected string _apiKey;

    [SerializeField]
    [Tooltip("ID of the graph to start the playthrough from.")]
    protected string _startGraphReferenceId;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Configuration of the Speech output.")]
    protected SpeechOptions _speechOptions;

    [SerializeField]
    [Tooltip("Configuration of the speech-to-text")]
    protected SpeechRecognitionOptions _speechRecognitionOptions;

    [SerializeField]
    [Tooltip("Activate internal Charisma-specific logging.")]
    protected bool _enableLogging;

    protected Playthrough _playthrough;
    protected string _conversationUuid;
    protected Playthrough.MessageDelegate _onMessageCallback;

    protected virtual void Start()
    {
        Logger.logEnabled = _enableLogging;
    }

    public void LoadPlaythrough()
    {
        // We use these settings to create a play-through token.
        StartCoroutine(CharismaAPI.CreatePlaythroughToken(new CreatePlaythroughTokenParams(_storyId,_storyVersion, _apiKey), callback: (tokenResponse) =>
        {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(tokenResponse.Token, callback: conversationUuid =>
            {
                // We'll cache our conversation Id since we need this to send replies and other events to Charisma.
                _conversationUuid = conversationUuid;

                // We can now create a new charisma object and pass it our token.
                _playthrough = new Playthrough(
                    tokenResponse.Token,
                    tokenResponse.PlaythroughUuid,
                    _speechOptions,
                    _speechRecognitionOptions
                );

                OnPlaythroughLoaded(tokenResponse, conversationUuid);
            }));
        }));
    }

    /// <summary>
    /// Starts the playthrough.
    /// This will begin the relevant active substory, and start sending messages back to the local Unity session.
    /// This message callback can be set via the SetOnMessageCallback function.
    /// </summary>
    protected void StartPlaythrough()
    {
        if (!IsPlaythroughLoaded())
        {
            Logger.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
        _playthrough.Connect(() =>
        {
            Logger.Log("Connecting to playthrough.");

            // We can now subscribe to message events from charisma.
            Logger.Log("Subscribed to messages.");

            // We can now subscribe to message events from charisma.
            _playthrough.OnMessage += OnMessageReceived;

            Logger.Log("Starting playthrough.");

            // In the start function, we pass the conversationId we cached earlier.
            _playthrough.Start(_conversationUuid, null , _startGraphReferenceId);
        });
    }

    /// <summary>
    /// Sends an action to the current playthrough.
    /// </summary>
    /// <param name="reply"></param>
    public void SetAction(string action)
    {
        if (!IsPlaythroughLoaded())
        {
            Logger.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // Send an action to our current conversation.
        _playthrough.Action(_conversationUuid, action);
    }

    protected abstract void OnPlaythroughLoaded(CreatePlaythroughTokenResponse tokenResponse, string conversationUuid);

    protected abstract void OnMessageReceived(MessageEvent message);

    /// <summary>
    /// Returns whether the Charisma playthrough has succesfully created
    /// </summary>
    protected bool IsPlaythroughLoaded()
    {
        return _playthrough != default;
    }

    private void OnApplicationQuit()
    {
        MainThreadDispatcher.Instance.Dispose();
    }
}
