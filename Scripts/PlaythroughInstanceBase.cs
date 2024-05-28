using System;
using CharismaSDK;
using CharismaSDK.Events;
using UnityEngine;

public abstract class PlaythroughInstanceBase : MonoBehaviour
{
    protected Playthrough _playthrough;
    protected string _conversationUuid;
    protected Playthrough.MessageDelegate _onMessageCallback;

    [Header("Charisma")]
    [Tooltip("Playthrough parameters - these are collected on Scene start currently.")]
    public CreatePlaythroughTokenParams ConnectionTokenParams;

    [SerializeField] [Tooltip("ID of the graph to start the playthrough from.")]
    protected string _startGraphReferenceId;

    [Tooltip("Activate internal Charisma-specific logging.")]
    [SerializeField] protected bool _enableLogging;

    protected virtual void Start()
    {
        CharismaSDK.Logger.logEnabled = _enableLogging;
    }

    public void LoadPlaythrough()
    {
        // We use these settings to create a play-through token.
        StartCoroutine(CharismaAPI.CreatePlaythroughToken(ConnectionTokenParams, callback: (tokenResponse) =>
        {
            // Once we receive the callback with our token, we can create a new conversation.
            StartCoroutine(CharismaAPI.CreateConversation(tokenResponse.Token, callback: conversationUuid =>
            {
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
            Debug.Log("Playthrough was not loaded. Please call LoadPlaythrough() first.");
            return;
        }

        // We can now connect to Charisma. Once we receive the ready callback, we can start our play-through.
        _playthrough.Connect(() =>
        {
            Debug.Log("Ready!");

            // In the start function, we pass the conversationId we cached earlier.
            _playthrough.Start(_conversationUuid, startGraphReferenceId: _startGraphReferenceId);
        });

        // On message callback needs to be assigned.
        if (_onMessageCallback != default)
        {
            // We can now subscribe to message events from charisma.
            _playthrough.OnMessage += HandleMessage;
        }
    }

    protected abstract void OnPlaythroughLoaded(CreatePlaythroughTokenResponse tokenResponse, string conversationUuid);

    protected virtual void OnMessageReceived(MessageEvent message)
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
            _onStoryEnd.Invoke();
            _playthrough.Disconnect();
        }
    }

    /// <summary>
    /// Returns whether the Charisma playthrough has succesfully been loaded
    /// </summary>
    public bool IsPlaythroughLoaded()
    {
        return _playthrough != default;
    }


}
