using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using CharismaSDK;
using CharismaSDK.Events;
using CharismaSDK.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Logger = CharismaSDK.Logger;

/// <summary>
/// This script demonstrates a simple interaction with Charisma
/// </summary>
public class ExamplePlaythroughInstance : PlaythroughInstanceBase
{
    [SerializeField] private bool _useSpeech;
    [SerializeField] private AudioSource _audioSource;
    
    [Header(header: "UI")] 
    [SerializeField] private Text _charismaChatText;
    [SerializeField] private Button _sendButton;
    [SerializeField] private Button _micButton;
    [SerializeField] private Button _tapButton;
    [SerializeField] private InputField _actionField;
    [SerializeField] private Button _sendActionButton;
    [SerializeField] private InputField _memoryKeyField;
    [SerializeField] private InputField _memoryValueField;
    [SerializeField] private Button _setMemoryButton;
    [SerializeField] private InputField _chatInputField;
    [SerializeField] private ConnectionStateDisplay _connectionStateDisplay;

    private List<string> _chatMessages = new List<string>(); 
    
    // STT
    private List<string> _recognizedSpeechTextList = new List<string>();
    private string _currentRecognizedText;
    
    private string CurrentRecognizedText
    {
        get
        {
            var text = new StringBuilder();

            foreach (var speechLine in _recognizedSpeechTextList)
            {
                text.Append(speechLine + " ");
            }

            text.Append(_currentRecognizedText);

            return text.ToString();
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        LoadPlaythrough();
        
        _sendButton.onClick.AddListener(SendPlayerMessage);
        _tapButton.onClick.AddListener(Tap);
        _sendActionButton.onClick.AddListener(SetAction);
        _setMemoryButton.onClick.AddListener(SetMemory);
        
        // Mic button handled with unity pointer events directly to get up/down states
        AddEvent(_micButton.gameObject, EventTriggerType.PointerDown, OnMicButtonDown);
        AddEvent(_micButton.gameObject, EventTriggerType.PointerUp, OnMicButtonUp);
    }

    private void OnMicButtonDown(BaseEventData obj)
    {
        SetPlaythroughToListening(true);
    }
    
    private void OnMicButtonUp(BaseEventData obj)
    {
        SetPlaythroughToListening(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }
    
    private void AddEvent(GameObject obj, EventTriggerType type, System.Action<BaseEventData> callback)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(callback.Invoke);
        trigger.triggers.Add(entry);
    }
    
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        
        if (_playthrough != default)
        {
            _playthrough.OnSpeechRecognitionResult -= OnSpeechRecognitionResult;
            _playthrough.Disconnect();
        }
    }
    
    public void SendPlayerMessage()
    {
        if (string.IsNullOrEmpty(value: _chatInputField.text))
        {
            return;
        }

        AddChatMesssage($"Player: {_chatInputField.text}");
        
        // Send the text of our input field to Charisma.
        _playthrough.Reply(_conversationUuid, _chatInputField.text);
        _chatInputField.text = string.Empty;
    }

    public void Tap()
    {
        base.Tap();
        AddChatMesssage("TAP");
    }
    
    public void SetAction()
    {
        var action = _actionField.text;
        base.SetAction(action);
        AddChatMesssage($"SENT ACTION: {action}");
    }

    public void SetMemory()
    {
        var key = _memoryKeyField.text;
        var value = _memoryValueField.text;
        base.SetMemory(key,value);
        AddChatMesssage($"SET MEMORY: {key} | {value}");
    }

    protected override void OnPlaythroughLoaded(CreatePlaythroughTokenResponse tokenResponse, string conversationUuid)
    {
        if (_connectionStateDisplay != default)
        {
            _playthrough.OnConnectionStateChange += _connectionStateDisplay.SetResultState;
        }
        
        _playthrough.OnSpeechRecognitionResult += OnSpeechRecognitionResult;

        StartPlaythrough();
    }

    protected override void OnMessageReceived(MessageEvent message)
    {
        Logger.Log(message);
        
        // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
        if (message.messageType == MessageType.panel)
        {
            return;
        }
        
        // We can't generate speech or access character & text data so we return after we have checked if this is the end of the story.
        if (message.endStory)
        {
            _playthrough.Disconnect();
            return;
        }

        if (_useSpeech && message.message.speech.audio.Length > 0)
        {
            // Once we have received a message character message, we might want to play the audio. To do this we run the GetClip method and wait for the callback which contains our audio clip, then pass it to the audio player.
            CharismaAudio.GetAudioClip(message.message.speech.encoding, message.message.speech.audio,
                onAudioGenerated: (clip =>
                {
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }));
        }

        var tapContinueStatus = message.tapToContinue ? "(TAP TO CONTINUE)" : "";
        AddChatMesssage($"{message.message.character?.name}: {message.message?.text} {tapContinueStatus}");
    }
    
    private void OnSpeechRecognitionResult(SpeechRecognitionResult message)
    {
        _currentRecognizedText = message.text;

        if (message.isFinal)
        {
            _recognizedSpeechTextList.Add(_currentRecognizedText);
            _currentRecognizedText = "";
        }

        _chatInputField.text = CurrentRecognizedText;
    }

    private void SetPlaythroughToListening(bool listening)
    {
        if (listening)
        {
            _recognizedSpeechTextList.Clear();
            _playthrough.StartSpeechRecognition(this.gameObject);
        }
        else
        {
            _playthrough.StopSpeechRecognition();
        }
    }

    private void AddChatMesssage(string message)
    {
        _chatMessages.Add(message);

        if (_chatMessages.Count > 20)
        {
            _chatMessages.RemoveAt(0);
        }
        
        UpdateChat();
    }

    private void UpdateChat()
    {
        var chatOutputString = "";
        foreach (var message in _chatMessages)
        {
            chatOutputString += $"{message}\n";
        }
        
        // Trim last "\n" out of the string for improved formatting
        var trimmedOutput = chatOutputString.TrimEnd('\n');
        _charismaChatText.text = trimmedOutput;
    }
}