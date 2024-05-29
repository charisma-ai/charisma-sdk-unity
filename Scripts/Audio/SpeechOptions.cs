using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSDK.Audio
{
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
            _audioOutput = output;
            _encoding = encoding;
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
