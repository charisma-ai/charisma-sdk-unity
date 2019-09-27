using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
#pragma warning disable 618

namespace CharismaSDK
{
    public class Audio
    {
        [JsonConstructor]
        public Audio(byte[] data, string type, string url)
        {
            this.Data = data;
            this.Type = type;
            this.Url = url;
        }

        public void GenerateAudio(SpeechOptions options, Action<AudioClip> onAudioGenerated)
        {
            if (!Data.Any())
            {
                Debug.LogError("There was no Audio Data to generate from. Check yor audio settings!");  
                return;
            }
            
            CoroutineConsumer.Instance.Consume(Generate(options, Data, onAudioGenerated));
         }
        private IEnumerator Generate(SpeechOptions options, byte[] data, Action<AudioClip> action)
        {
            if (options.encoding == "mp3")
            {
                CharismaLogger.Log("Charisma: Generating audio");

                var tempFile = Application.persistentDataPath + "/bytes.mp3";

                if (Data != null)
                    System.IO.File.WriteAllBytes(tempFile, Data);

                var clip = new WWW("file://" + tempFile);
                while (!clip.isDone)
                    yield return null;
                
                // Generate the clip
                Clip = CharismaUtilities.FromMp3Data(clip.bytes);
                
                action.Invoke(Clip);
            }
            else
            {
                CharismaLogger.Log("Charisma: Generating audio");

                var tempFile = Application.persistentDataPath + "/bytes.ogg";

                if (Data != null)
                    System.IO.File.WriteAllBytes(tempFile, Data);

                var clip = new WWW("file://" + tempFile);
                while (!clip.isDone)
                    yield return null;
                
                // Generate the clip
                Clip = clip.GetAudioClip(false, false, AudioType.OGGVORBIS);
                
                action.Invoke(Clip);
            }
        }

        /// <summary>
        /// The type of audio.
        /// </summary>
        public string Type { get; }
        
        /// <summary>
        /// Raw bytes.
        /// </summary>
        public byte[] Data { get; }
        
        /// <summary>
        /// Generated audio clip.
        /// </summary>
        public AudioClip Clip { get; set; }
        
        /// <summary>
        /// Url of this audio clip. Only available if Speech setting is set to Url.
        /// </summary>
        public string Url { get; set; }
    }

    public class Wav
    {

        // convert two bytes to one float in the range -1 to 1
        private static float BytesToFloat(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            var s = (short) ((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        private static int BytesToInt(byte[] bytes, int offset = 0)
        {
            var value = 0;
            for (var i = 0; i < 4; i++)
            {
                value |= ((int) bytes[offset + i]) << (i * 8);
            }

            return value;
        }


        public float[] LeftChannel { get; private set; }
        public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; private set; }
        public int Frequency { get; private set; }

        public Wav(byte[] wav)
        {

            // Determine if mono or stereo
            ChannelCount = wav[22]; // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get the frequency
            Frequency = BytesToInt(wav, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            var pos = 12; // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                var chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }

            pos += 8;

            // Pos is now positioned to start of actual sound data.
            SampleCount = (wav.Length - pos) / 2; // 2 bytes per sample (16 bit sound mono)
            if (ChannelCount == 2) SampleCount /= 2; // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            LeftChannel = new float[SampleCount];
            RightChannel = ChannelCount == 2 ? new float[SampleCount] : null;

            // Write to double array/s:
            var i = 0;
            while (pos < wav.Length)
            {
                LeftChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
                if (ChannelCount == 2)
                {
                    if (RightChannel != null) RightChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                }

                i++;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]"
                , LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
        }
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
        [SerializeField]private AudioOutput _audioOutput;
        [SerializeField]private Encoding _encoding;

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
