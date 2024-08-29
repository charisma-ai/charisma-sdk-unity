using UnityEngine;

namespace CharismaSDK.Audio
{
    public abstract class MicrophoneBase : MonoBehaviour
    {
        protected const double NORMALIZED_FLOAT_TO_16BIT_CONVERSIONFACTOR = 0x7FFF + 0.4999999999999999;

        protected string _micString;
        protected AudioClip _clip;
        protected int _readHead = 0;
        protected int _microphoneId;
        protected int _sampleRate;

        public delegate void MicCallbackDelegate(byte[] buf);
        public MicCallbackDelegate MicrophoneCallback;

        public void Initialize(int microphoneId, int sampleRate = 16000)
        {
            _microphoneId = microphoneId;
            _sampleRate = sampleRate;
        }

        public abstract void StartListening();

        public abstract void StopListening();
    }
}
