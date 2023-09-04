using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace CharismaSDK.Sound
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
}
