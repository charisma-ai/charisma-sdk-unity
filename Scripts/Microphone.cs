using System;
using UnityEngine;

namespace CharismaSDK {

    public class Microphone : MonoBehaviour
    {
        public delegate void MicCallbackDelegate(byte[] buf);
        public MicCallbackDelegate micCallbackDelegate;

        private const double NormalizedFloatTo16BitConversionFactor = 0x7FFF + 0.4999999999999999;

        void FixedUpdate()
        {
            if (clip != null)
            {
                ReadMicrophoneAudio();
            }
        }

        void ReadMicrophoneAudio()
        {
            int writeHead = UnityEngine.Microphone.GetPosition(MicString);

            if (readHead == writeHead || !UnityEngine.Microphone.IsRecording(MicString))
            {
                return;
            }

            // Say audio.clip.samples (S)  = 100
            // if w=1, r=0, we want 1 sample.  ( S + 1 - 0 ) % S = 1 YES
            // if w=0, r=99, we want 1 sample.  ( S + 0 - 99 ) % S = 1 YES
            int nFloatsToGet = (clip.samples + writeHead - readHead) % clip.samples;

            float[] floatBuffer = new float[nFloatsToGet];

            // If the read length from the offset is longer than the clip length,
            //   the read will wrap around and read the remaining samples
            //   from the start of the clip.
            clip.GetData(floatBuffer, readHead);
            readHead = (readHead + nFloatsToGet) % clip.samples;

            byte[] byteBuffer = new byte[floatBuffer.Length * 2];
            // convert 1st channel of audio from floating point to 16 bit packed into a byte array
            // reference: https://github.com/naudio/NAudio/blob/ec5266ca90e33809b2c0ceccd5fdbbf54e819568/Docs/RawSourceWaveStream.md#playing-from-a-byte-array
            for (int i = 0; i < floatBuffer.Length; i++)
            {
                short sample = (short)(floatBuffer[i] * NormalizedFloatTo16BitConversionFactor);
                byte[] bytes = BitConverter.GetBytes(sample);
                byteBuffer[i * 2] = bytes[0];
                byteBuffer[i * 2 + 1] = bytes[1];
            }


            if (micCallbackDelegate != null)
            {
                micCallbackDelegate(byteBuffer);
            }
        }

        private string MicString;
        private AudioClip clip;
        private int readHead = 0;

        public void StartListening()
        {
            if (UnityEngine.Microphone.devices.Length == 0)
            {
                Logger.LogError("Could not start microphone as there are no devices.");
                return;
            }

            MicString = UnityEngine.Microphone.devices[0];
            clip = UnityEngine.Microphone.Start(MicString, true, 10, 16000);
        }

        public void StopListening()
        {
            if (!clip)
            {
                Logger.LogError("Could not stop microphone as it was not started.");
                return;
            }

            UnityEngine.Microphone.End(MicString);
            MicString = null;
            clip = null;
        }
    }
}