using UnityEngine;
using UnityEngine.UI;
using CharismaSDK;
using CharismaSDK.Events;
using CharismaSDK.Audio;
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
    [SerializeField] private Button _sendButton;
    [SerializeField] private InputField _inputField;
    [SerializeField] private Text _textField;

    [SerializeField] private ConnectionStateDisplay _connectionStateDisplay;
    
    protected override void Start()
    {
        base.Start();
        
        LoadPlaythrough();

        // Bind the SendPlayerMessage function to the UI button.
        _sendButton.onClick.AddListener(call: SendPlayerMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (_playthrough != default)
        {
            _playthrough.OnMessage -= OnMessageReceived;
            
            _playthrough.Disconnect();
        }
    }
    
    public void SendPlayerMessage()
    {
        if (string.IsNullOrEmpty(value: _inputField.text))
        {
            return;
        }

        // Send the text of our input field to Charisma.
        _playthrough.Reply(conversationUuid: _conversationUuid, text: _inputField.text);

        _inputField.text = string.Empty;
    }

    protected override void OnPlaythroughLoaded(CreatePlaythroughTokenResponse tokenResponse, string conversationUuid)
    {
        if (_connectionStateDisplay != default)
        {
            _playthrough.OnConnectionStateChange += _connectionStateDisplay.SetResultState;
        }

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

        _textField.text = ($"{message.message.character?.name}: {message.message?.text}");
    }
}