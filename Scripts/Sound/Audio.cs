using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

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

# if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string SaveBufferAsBlob(byte[] buffer, int bufferSize);
# endif

        private static IEnumerator GenerateAudio(string encoding, byte[] bytes, Action<AudioClip> callback)
        {
            if (bytes == null)
            {
                Debug.LogError("`bytes` provided to `GenerateAudio` was `null`, aborting.");
                yield return null;
            }

            AudioType audioType = GetAudioType(encoding);

# if UNITY_WEBGL && !UNITY_EDITOR
            // We're not allowed to use local file storage in browser contexts,
            // so instead we save it as a blob.
            string uri = SaveBufferAsBlob(bytes, bytes.Length);
# else
            string tempFile = Application.persistentDataPath + "/bytes." + encoding;
            File.WriteAllBytes(tempFile, bytes);
            string uri = "file://" + tempFile;
# endif

            var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            yield return request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            callback.Invoke(clip);
        }

        private static IEnumerator GenerateAudio(string encoding, string url, Action<AudioClip> callback)
        {
            AudioType audioType = GetAudioType(encoding);

            var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            yield return request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            callback.Invoke(clip);
        }

        #endregion
    }
}
