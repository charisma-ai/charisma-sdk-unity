using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private AudioOutput _audioOutput = AudioOutput.Buffer;
        /// <summary>
        /// Defaults to a list of all audio encodings that Unity supports out of the box through the
        /// `DownloadHandlerAudioClip.GetContent` method.
        /// </summary>
        [SerializeField] private List<Encoding> _encoding = new List<Encoding> {
            Encoding.Ogg,
            Encoding.Mp3,
            Encoding.Wav
        };

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

        public SpeechOptions()
        {
        }

        /// <summary>
        /// Set the audio information coming back from Charisma.
        /// </summary>
        /// <param name="output">What output format to use</param>
        /// <param name="encoding">What encoding to use</param>
        public SpeechOptions(AudioOutput output, List<Encoding> encoding)
        {
            this._audioOutput = output;
            this._encoding = encoding;
        }

        public List<string> encoding
        {
            get
            {
                List<string> encodings = new List<string>();
                foreach(var entry in _encoding) {
                    if (entry == Encoding.Mp3)
                    {
                        encodings.Add("mp3");
                    }
                    else if(entry == Encoding.Ogg)
                    {
                        encodings.Add("ogg");
                    }
                    else if (entry == Encoding.Wav)
                    {
                        encodings.Add("wav");
                    }
                }
                return encodings;
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
    }


}
