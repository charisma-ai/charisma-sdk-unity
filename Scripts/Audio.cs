using System;
using System.Collections;
using System.IO;
using System.Linq;
// using NAudio.Wave;
using UnityEngine;
using UnityEngine.Networking;

namespace CharismaSDK
{
    public class Audio
    {
        #region Static Methods

        //public static AudioClip Mp3BytesToAudioClip(byte[] data)
        //{
        //    // Load the data into a stream
        //    var mp3Stream = new MemoryStream(data);
        //    // Convert the data in the stream to WAV format
        //    var mp3Audio = new Mp3FileReader(mp3Stream);
        //    var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Audio);
        //    // Convert to WAV data
        //    var wav = new Wav(AudioMemStream(waveStream).ToArray());

        //    var audioClip = AudioClip.Create("CharismaSpeech", wav.SampleCount, 1, wav.Frequency, false);
        //    audioClip.SetData(wav.LeftChannel, 0);
        //    // Return the clip
        //    return audioClip;
        //}

        //private static MemoryStream AudioMemStream(WaveStream waveStream)
        //{
        //    var outputStream = new MemoryStream();

        //    using (var waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
        //    {
        //        var bytes = new byte[waveStream.Length];
        //        waveStream.Position = 0;
        //        waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
        //        waveFileWriter.Write(bytes, 0, bytes.Length);
        //        waveFileWriter.Flush();
        //    }
        //    return outputStream;
        //}

        #endregion

        /// <summary>
        /// Generates an `AudioClip`.
        /// </summary>
        /// <param name="encoding"> The encoding of the audio clip</param>
        /// <param name="bytes"> The bytes of the audio clip</param>
        /// <param name="onAudioGenerated"> Callback containing the generated audio clip</param>
        /// <exception cref="NullReferenceException"></exception>
        public static void GetAudioClip(string encoding, byte[] bytes, Action<AudioClip> onAudioGenerated)
        {
            if (!bytes.Any())
                throw new NullReferenceException("There was no audio data to generate from. Check your audio settings.");

            MainThreadConsumer.Instance.Consume(GenerateAudio(encoding, bytes, onAudioGenerated));
        }

        private static IEnumerator GenerateAudio(string encoding, byte[] bytes, Action<AudioClip> callback)
        {
            AudioType selectedEncoding;
            if (encoding == "mp3")
            {
                selectedEncoding = AudioType.MPEG;
            }
            else if (encoding == "ogg")
            {
                selectedEncoding = AudioType.OGGVORBIS;
            }
            else if (encoding == "wav")
            {
                selectedEncoding = AudioType.WAV;
            }
            else
            {
                throw new NotImplementedException();
            }

            var tempFile = Application.persistentDataPath + "/bytes.ogg";

            if (bytes != null)
                File.WriteAllBytes(tempFile, bytes);

            var request = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFile, selectedEncoding);
            yield return request.SendWebRequest();
            while (!request.isDone)
                yield return null;

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            callback.Invoke(clip);
        }
    }

    [Serializable]
    public class SpeechOptions
    {
        [SerializeField] private AudioOutput _audioOutput;
        [SerializeField] private Encoding _encoding;

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
        public SpeechOptions(AudioOutput output, Encoding encoding)
        {
            this._audioOutput = output;
            this._encoding = encoding;
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
                        Debug.LogError("Unknown audio format");
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
                    //case AudioOutput.Url:
                    //    return "url";
                    case AudioOutput.Buffer:
                        return "buffer";
                    default:
                        Debug.LogError("Unknown output method");
                        return null;
                }
            }
        }
    }


}
