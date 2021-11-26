using System;
using System.Collections;
using System.IO;
using System.Linq;
using NAudio.Wave;
using Newtonsoft.Json;
using UnityEngine;

// Removes the WWW obsolete warning.
#pragma warning disable 618

namespace CharismaSdk
{
    public class Audio
    {
        #region Static Methods

        public static AudioClip FromMp3Data(byte[] data)
        {
            // Load the data into a stream
            var mp3Stream = new MemoryStream(data);
            // Convert the data in the stream to WAV format
            var mp3Audio = new Mp3FileReader(mp3Stream);
            var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Audio);
            // Convert to WAV data
            var wav = new Wav(AudioMemStream(waveStream).ToArray());

            var audioClip = AudioClip.Create("CharismaSpeech", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            // Return the clip
            return audioClip;
        }

        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            var outputStream = new MemoryStream();

            using (var waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                var bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }

        #endregion

        [JsonConstructor]
        public Audio(byte[] data, string type, string url)
        {
            this.Data = data;
            this.Type = type;
            this.Url = url;
        }

        /// <summary>
        /// Returns the Url pointing to the audio file.
        /// </summary>
        /// <returns></returns>
        public string GetClip()
        {
            if (string.IsNullOrEmpty(Url))
                throw new NullReferenceException("There was no audio Url available. Check your audio settings.");

            return Url;
        }

        /// <summary>
        /// Generates an audioClip.
        /// </summary>
        /// <param name="options"> Provide the speech options used with Charisma</param>
        /// <param name="onAudioGenerated"> Callback containing the generated audio clip</param>
        /// <exception cref="NullReferenceException"></exception>
        public void GetClip(SpeechOptions options, Action<AudioClip> onAudioGenerated)
        {
            if (!Data.Any() && onAudioGenerated != null)
                throw new NullReferenceException("There was no Audio Data to generate from. Check your audio settings.");

            MainThreadConsumer.Instance.Consume(GenerateAudio(options, Data, onAudioGenerated));
        }
        private IEnumerator GenerateAudio(SpeechOptions options, byte[] data, Action<AudioClip> action)
        {
            switch (options.encoding)
            {
                case "mp3":
                    {
                        var tempFile = Application.persistentDataPath + "/bytes.mp3";

                        if (Data != null)
                            File.WriteAllBytes(tempFile, Data);

                        var clip = new WWW("file://" + tempFile);
                        while (!clip.isDone)
                            yield return null;

                        // GenerateAudio the clip
                        Clip = FromMp3Data(clip.bytes);

                        action.Invoke(Clip);

                        CharismaLogger.Log("Generated audio");
                        break;
                    }
                case "ogg":
                    {
                        var tempFile = Application.persistentDataPath + "/bytes.ogg";

                        if (Data != null)
                            File.WriteAllBytes(tempFile, Data);

                        var clip = new WWW("file://" + tempFile);
                        while (!clip.isDone)
                            yield return null;

                        // GenerateAudio the clip
                        Clip = clip.GetAudioClip(false, false, AudioType.OGGVORBIS);

                        action.Invoke(Clip);

                        CharismaLogger.Log("Generated audio");
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Audio type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Raw bytes.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Generated audio clip.
        /// </summary>
        public AudioClip Clip { get; private set; }

        /// <summary>
        /// Url of this audio clip. Only available if Speech setting is set to Url.
        /// </summary>
        public string Url { get; }
    }


    public class Speech
    {
        [JsonConstructor]
        public Speech(Audio audio, float duration)
        {
            this.Audio = audio;
            this.Duration = duration;
        }

        /// <summary>
        /// Audio information from this response.
        /// </summary>
        public Audio Audio { get; }

        /// <summary>
        /// Duration of the audio from this response.
        /// </summary>
        public float Duration { get; set; }
    }

    [Serializable]
    public class SpeechOptions
    {
        [Header("Wav only available on Windows")]
        [SerializeField] private AudioOutput _audioOutput;
        [SerializeField] private Encoding _encoding;

        public enum Encoding
        {
            Wav,
            Ogg
        }

        public enum AudioOutput
        {
            Url,
            Buffer
        }

        /// <summary>
        /// Set the audio information coming back from Charisma.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="encoding">Wav is only available on Windows.</param>
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
                    case Encoding.Wav:
                        return "mp3";
                    case Encoding.Ogg:
                        return "ogg";
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
                    case AudioOutput.Url:
                        return "url";
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
