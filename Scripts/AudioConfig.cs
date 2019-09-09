using System;
using UnityEngine;

namespace CharismaSDK
{
    [Serializable]
    public class AudioConfig
    {
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

        [Tooltip("Mp3 currently only available on Windows platforms")][Header("Audio Encoding (Platform dependent)")]
        [SerializeField] private Encoding _encoding;
        [Tooltip("Output type")]
        [SerializeField] private AudioOutput _audioOutput;

        public Encoding EncodingOption => _encoding;
        public AudioOutput AudioOutputOption => _audioOutput;
        
        public AudioConfig(Encoding encoding, AudioOutput audioOutput)
        {
            this._encoding = encoding;
            this._audioOutput = audioOutput;
        }

        public string AudioEncoding
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

        public string Output
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
