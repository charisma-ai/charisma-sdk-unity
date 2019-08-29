using UnityEngine;

namespace CharismaSDK
{
    public class SpeechStartOptions
    {
        public string output;
        public string encoding;

        public SpeechStartOptions(string output, string encoding)
        {
            this.output = output;
            this.encoding = encoding;
        }
    }
}
