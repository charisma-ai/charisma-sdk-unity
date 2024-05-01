using System;
using UnityEngine;

namespace CharismaSDK.Sound
{

    public class Microphone : MonoBehaviour
    {
        public delegate void MicCallbackDelegate(byte[] buf);

        public MicCallbackDelegate MicrophoneCallback;

        private const double NORMALIZED_FLOAT_TO_16BIT_CONVERSIONFACTOR = 0x7FFF + 0.4999999999999999;

        private string _micString;
        private AudioClip _clip;
        private int _readHead = 0;

        private int _microphoneId;

        private int _sampleRate;

        void FixedUpdate()
        {
            if (_clip != null)
            {
                ReadMicrophoneAudio();
            }
        }

        internal void Initialize(int microphoneId, int sampleRate = 16000)
        {
            _microphoneId = microphoneId;
            _sampleRate = sampleRate;
        }

        void ReadMicrophoneAudio()
        {
            int writeHead = UnityEngine.Microphone.GetPosition(_micString);

            if (_readHead == writeHead || !UnityEngine.Microphone.IsRecording(_micString))
            {
                return;
            }

            // Say audio.clip.samples (S)  = 100
            // if w=1, r=0, we want 1 sample.  ( S + 1 - 0 ) % S = 1 YES
            // if w=0, r=99, we want 1 sample.  ( S + 0 - 99 ) % S = 1 YES
            int nFloatsToGet = (_clip.samples + writeHead - _readHead) % _clip.samples;

            float[] floatBuffer = new float[nFloatsToGet];

            // If the read length from the offset is longer than the clip length,
            //   the read will wrap around and read the remaining samples
            //   from the start of the clip.
            _clip.GetData(floatBuffer, _readHead);
            _readHead = (_readHead + nFloatsToGet) % _clip.samples;

            byte[] byteBuffer = new byte[floatBuffer.Length * 2];
            // convert 1st channel of audio from floating point to 16 bit packed into a byte array
            // reference: https://github.com/naudio/NAudio/blob/ec5266ca90e33809b2c0ceccd5fdbbf54e819568/Docs/RawSourceWaveStream.md#playing-from-a-byte-array
            for (int i = 0; i < floatBuffer.Length; i++)
            {
                short sample = (short)(floatBuffer[i] * NORMALIZED_FLOAT_TO_16BIT_CONVERSIONFACTOR);
                byte[] bytes = BitConverter.GetBytes(sample);
                byteBuffer[i * 2] = bytes[0];
                byteBuffer[i * 2 + 1] = bytes[1];
            }


            if (MicrophoneCallback != null)
            {
                MicrophoneCallback(byteBuffer);
            }
        }

        public void StartListening()
        {
            if (UnityEngine.Microphone.devices.Length == 0)
            {
                Logger.LogError("Could not start microphone as there are no devices.");
                return;
            }

            _readHead = 0;
            _micString = UnityEngine.Microphone.devices[_microphoneId];
            _clip = UnityEngine.Microphone.Start(_micString, true, 10, _sampleRate);
        }

        public void StopListening()
        {
            if (!_clip)
            {
                Logger.LogError("Could not stop microphone as it was not started.");
                return;
            }

            UnityEngine.Microphone.End(_micString);
            _micString = null;
            _clip = null;
        }
    }
}
