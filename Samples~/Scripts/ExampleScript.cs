using CharismaSDK;
using CharismaSDK.Events;
using System.Collections;
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

    private void Awake()
    {
        StartCoroutine(Bind());
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }

    private IEnumerator Bind()
    {
        var instance = SimplePlaythrough.Instance;

        if (instance != default)
        {
            _playthrough = instance;

            _playthrough.OnMessage += OnMessageReceived;
            _replyButton.onClick.AddListener(SendPlayerMessage);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            yield return Bind();
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

    private void OnMessageReceived(MessageEvent message)
    {
        TryOutputAudio(message);
        TryOutputText(message);
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
                Audio.GetAudioClip(_playthrough.SpeechOptions.encoding.ToString(), message.message.speech.audio, onAudioGenerated: (clip =>
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
