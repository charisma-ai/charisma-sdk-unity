using System;
using CharismaSDK;
using CharismaSDK.Audio;
using CharismaSDK.Events;
using UnityEngine;
using Logger = CharismaSDK.Logger;

public abstract class PlaythroughInstanceBase : MonoBehaviour
{
    [Header("Charisma")]
    [SerializeField]
    [Tooltip("Playthrough parameters used to connect to Charisma backend and start a playthrough")]
    protected CreatePlaythroughTokenParams _connectionTokenParams;

    [SerializeField]
    [Tooltip("ID of the graph to start the playthrough from.")]
    protected string _startGraphReferenceId;

    [SerializeField]
    [Tooltip("Index of the scene to start the playthrough from.")] [Min(1)]
    protected int _startFromScene;

    [SerializeField]
    [Tooltip("Configuration node of the Speech output.")]
    protected SpeechOptions _speechOptions;

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
        StartCoroutine(CharismaAPI.CreatePlaythroughToken(_connectionTokenParams, callback: (tokenResponse) =>
        {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(tokenResponse.Token, callback: conversationUuid =>
            {
                // We'll cache our conversation Id since we need this to send replies and other events to Charisma.
                _conversationUuid = conversationUuid;

                // We can now create a new charisma object and pass it our token.
                _playthrough = new Playthrough(
                    token: tokenResponse.Token,
                    playthroughUuid: tokenResponse.PlaythroughUuid,
                    _speechOptions
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
    public void StartPlaythrough()
    {
        if (!IsPlaythroughLoaded())
        {
            Logger.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
        _playthrough.Connect(() =>
        {
            Logger.Log("Playthrough ready to connect!");

            // We can now subscribe to message events from charisma.
            if (_onMessageCallback != default)
            {
                // We can now subscribe to message events from charisma.
                _playthrough.OnMessage += OnMessageReceived;
            }

            // In the start function, we pass the conversationId we cached earlier.
            _playthrough.Start(_conversationUuid, startGraphReferenceId: _startGraphReferenceId);
        });
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
}
