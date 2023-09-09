using CharismaSDK;
using CharismaSDK.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

internal class SimpleCharismaActor : MonoBehaviour
{
    private SpeechOptions _speechOptions;
    private bool _useSpeechOutput;

    [SerializeField]
    [Tooltip("Text source, used to print the synthesized speech on screen.")]
    private Text _textOutput;

    [SerializeField]
    [Tooltip("Audio source, used to output the synthesized speech. Only active if _useSpeech is toggled to true.")]
    private AudioSource _audioOutput;


    private void Awake()
    {
        StartCoroutine(Bind());
    }

    private IEnumerator Bind()
    {
        var instance = SimplePlaythrough.Instance;

        if (instance != default)
        {
            instance.OnMessage += SendMessage;
            SetSpeechOptions(instance.SpeechOptions);
            SetUseSpeechOutput(instance.UseSpeechOutput);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            yield return Bind();
        }
    }

    private void SendMessage(MessageEvent message)
    {
        TryOutputAudio(message);
        TryOutputText(message);
    }

    private void SetSpeechOptions(SpeechOptions speechOptions)
    {
        _speechOptions = speechOptions;
    }

    private void SetUseSpeechOutput(bool speechOutput)
    {
        _useSpeechOutput = speechOutput;
    }

    private void TryOutputAudio(MessageEvent message)
    {
        if (_useSpeechOutput)
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
                    Audio.GetAudioClip(message.message.speech.encoding, message.message.speech.audio, onAudioGenerated: (clip =>
                    {
                        _audioOutput.clip = clip;
                        _audioOutput.Play();
                    }));
                }
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