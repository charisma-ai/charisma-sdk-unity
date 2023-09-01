using System;
using UnityEngine;

namespace CharismaSDK.Sound
{
    [Serializable]
    public class SpeechRecognitionOptions
    {
        public enum SpeechRecognitionService
        {
            Unified,
            Google,
            Aws,
            Deepgram
        }

        public string Service
        {

            get
            {
                switch (_service)
                {
                    case SpeechRecognitionService.Unified:
                        return "unified";
                    case SpeechRecognitionService.Google:
                        return "unified:google";
                    case SpeechRecognitionService.Aws:
                        return "unified:aws";
                    case SpeechRecognitionService.Deepgram:
                        return "unified:deepgram";
                    default:
                        Logger.LogError("Unrecognised SpeechRecognitionService");
                        return null;
                }
            }
        }

        public string LanguageCode => _languageCode;

        public int SampleRate => _sampleRate;

        [SerializeField]
        private SpeechRecognitionService _service = SpeechRecognitionService.Unified;

        [SerializeField]
        private string _languageCode = "en-US";

        [SerializeField]
        private int _sampleRate = 16000;

        public SpeechRecognitionOptions()
        {

        }

        public SpeechRecognitionOptions(SpeechRecognitionService service, string languageCode, int sampleRate)
        {
            _service = service;
            _sampleRate = sampleRate;
            _languageCode = languageCode;
        }
    }
}
