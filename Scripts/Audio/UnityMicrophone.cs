using System;
using UnityEngine;

namespace CharismaSDK.Audio
{
#if !UNITY_WEBGL
    public class UnityMicrophone : MicrophoneBase
    {
        void FixedUpdate()
        {
            if (_clip != null)
            {
                ReadMicrophoneAudio();
            }
        }

        private void ReadMicrophoneAudio()
        {
            int writeHead = Microphone.GetPosition(_micString);

            if (_readHead == writeHead || !Microphone.IsRecording(_micString))
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

        public override void StartListening()
        {
            if (UnityEngine.Microphone.devices.Length == 0)
            {
                Logger.LogError("Could not start microphone as there are no devices.");
                return;
            }

            _readHead = 0;
            _micString = Microphone.devices[_microphoneId];
            _clip = Microphone.Start(_micString, true, 10, _sampleRate);
        }

        public override void StopListening()
        {
            if (!_clip)
            {
                Logger.LogError("Could not stop microphone as it was not started.");
                return;
            }

            Microphone.End(_micString);
            _micString = null;
            _clip = null;
        }
    }
#endif
}
