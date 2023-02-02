using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace CharismaSDK
{
    public class Audio
    {
        #region Static Methods

        /// <summary>
        /// Generates an `AudioClip` from a byte array.
        /// </summary>
        /// <param name="encoding">The encoding of the audio clip</param>
        /// <param name="bytes">The bytes of the audio clip</param>
        /// <param name="onAudioGenerated">Callback containing the generated audio clip</param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static void GetAudioClip(string encoding, byte[] bytes, Action<AudioClip> onAudioGenerated)
        {
            if (!bytes.Any())
            {
                throw new NullReferenceException("There was no audio data to generate from. Check your audio settings.");
            }

            MainThreadConsumer.Instance.Consume(GenerateAudio(encoding, bytes, onAudioGenerated));
        }

        /// <summary>
        /// Generates an `AudioClip` from a URL.
        /// </summary>
        /// <param name="encoding">The encoding of the audio clip</param>
        /// <param name="url">The URL of the audio clip</param>
        /// <param name="onAudioGenerated">Callback containing the generated audio clip</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void GetAudioClip(string encoding, string url, Action<AudioClip> onAudioGenerated)
        {
            MainThreadConsumer.Instance.Consume(GenerateAudio(encoding, url, onAudioGenerated));
        }

        private static AudioType GetAudioType(string encoding)
        {
            if (encoding == "mp3")
            {
                return AudioType.MPEG;
            }
            else if (encoding == "ogg")
            {
                return AudioType.OGGVORBIS;
            }
            else if (encoding == "wav")
            {
                return AudioType.WAV;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static IEnumerator GenerateAudio(string encoding, byte[] bytes, Action<AudioClip> callback)
        {
            AudioType audioType = GetAudioType(encoding);

            var tempFile = Application.persistentDataPath + "/bytes.ogg";

            if (bytes != null)
                File.WriteAllBytes(tempFile, bytes);

            var request = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFile, audioType);
            yield return request.SendWebRequest();
            while (!request.isDone)
                yield return null;

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            callback.Invoke(clip);
        }

        private static IEnumerator GenerateAudio(string encoding, string url, Action<AudioClip> callback)
        {
            AudioType audioType = GetAudioType(encoding);

            var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            yield return request.SendWebRequest();
            while (!request.isDone)
                yield return null;

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            callback.Invoke(clip);
        }

        #endregion
    }

    [Serializable]
    public class SpeechOptions
    {
        [SerializeField] private AudioOutput _audioOutput;
        [SerializeField] private Encoding _encoding;
        /// <summary>
        /// Id of the receiving microphone can be different on different devices.
        /// </summary>
        [SerializeField] private int _microphoneId;

        public enum Encoding
        {
            Mp3,
            Ogg,
            Wav
        }

        public enum AudioOutput
        {
            // TODO: Add support for URL output
            // Url,
            Buffer
        }

        /// <summary>
        /// Set the audio information coming back from Charisma.
        /// </summary>
        /// <param name="output">What output format to use</param>
        /// <param name="encoding">What encoding to use</param>
        /// <param name="microphoneId">Id of the receiving microphone</param>
        public SpeechOptions(AudioOutput output, Encoding encoding, int microphoneId)
        {
            this._audioOutput = output;
            this._encoding = encoding;
            this._microphoneId = microphoneId;
        }

        public string encoding
        {
            get
            {
                switch (_encoding)
                {
                    case Encoding.Mp3:
                        return "mp3";
                    case Encoding.Ogg:
                        return "ogg";
                    case Encoding.Wav:
                        return "wav";
                    default:
                        Logger.LogError("Unknown audio format");
                        return null;
                }
            }
        }

        public string output
        {
            get
            {
                switch (_audioOutput)
                {
                    // case AudioOutput.Url:
                    //     return "url";
                    case AudioOutput.Buffer:
                       return "buffer";
                    default:
                        Logger.LogError("Unknown output method");
                        return null;
                }
            }
        }

        public int microphoneId => _microphoneId;
    }
}
