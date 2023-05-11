using CharismaSDK;
using CharismaSDK.Events;
using UnityEngine;
using UnityEngine.UI;

public class ExampleScript : MonoBehaviour
{
    [SerializeField]
    private SimplePlaythrough _playthrough;

    [Header(header: "Input")]
    [SerializeField]
    private InputField _textInput;

    [SerializeField]
    private Button _replyButton;

    [Header(header: "Output")]
    [SerializeField]
    private Text _textOutput;

    [SerializeField]
    private AudioSource _audioOutput;


    // Start is called before the first frame update
    void Start()
    {
        _playthrough.SetOnLoadCallback(OnLoadPlaythrough);
        _playthrough.SetOnMessageCallback(OnMessageReceived);

        _playthrough.LoadPlaythrough();

        _replyButton.onClick.AddListener(SendPlayerMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }

    public void SendPlayerMessage()
    {
        if (string.IsNullOrEmpty(value: _textInput.text))
        {
            return;
        }

        // Send the text of our input field to Charisma.
        _playthrough.SendReply(_textInput.text);

        _textInput.text = string.Empty;
    }

    private void OnLoadPlaythrough()
    {
        _playthrough.StartPlaythrough();
    }

    private void OnMessageReceived(MessageEvent message)
    {
        Debug.Log(message);

        // If the message is a panel-node, we should operate on this data without trying to generate audio or access the text & character data of the node since panel-nodes have neither.
        if (message.messageType == MessageType.panel)
        {
            Debug.Log("This is a panel node");
        }
        else
        {
            TryOutputAudio(message);
            TryOutputText(message);
        }

        // If this is the end of the story, we disconnect from Charisma.
        if (message.endStory)
        {
            _playthrough.Playthrough.Disconnect();
        }
    }

    private void TryOutputAudio(MessageEvent message)
    {
        if (_audioOutput == default)
        {
            Debug.LogWarning("Audio output has not been assigned. Please set _audioOutput field.");
        }
        else
        {
            if (message.message.speech == default)
            {
                Debug.LogWarning("Did not receive speech body. Failed to generate audio clip.");
            }
            else if (message.message.speech.audio.Length > 0)
            {
                // Once we have received a message character message, we might want to play the audio. To do this we run the GetClip method and wait for the callback which contains our audio clip, then pass it to the audio player.
                Audio.GetAudioClip(_playthrough.Playthrough.SpeechOptions.encoding.ToString(), message.message.speech.audio, onAudioGenerated: (clip =>
                {
                    _audioOutput.clip = clip;
                    _audioOutput.Play();
                }));
            }
        }
    }

    private void TryOutputText(MessageEvent message)
    {
        if (_textOutput == default)
        {
            Debug.LogWarning("Text output has not been assigned. Please set _textOutput field.");
        }
        else
        {
            _textOutput.text = ($"{message.message.character?.name}: {message.message?.text}");
        }
    }
}
